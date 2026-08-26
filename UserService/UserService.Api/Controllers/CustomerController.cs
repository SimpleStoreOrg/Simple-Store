using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
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
    public async Task<ActionResult<UserResponse>> GetAllCustomersAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.Send(new GetAllCustomersQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomerAsync(long id, UpdateCustomerRequest request)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(id, request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomerAsync(long id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}