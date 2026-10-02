using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Features.Customers.Commands;
using UserService.Application.Features.Customers.Queries;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet]
    public async Task<ActionResult<CustomerResponse>> GetAllCustomersAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.Send(new GetAllCustomersQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentCustomerAsync()
    {
        var customerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not Authorized");
        }

        long customerId = long.Parse(customerIdStr);

        var result = await _mediator.Send(new GetCustomerByIdQuery(customerId));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPut]
    public async Task<IActionResult> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(request));
        return Ok(result);
    }
}