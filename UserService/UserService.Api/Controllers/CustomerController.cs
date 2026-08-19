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
[Authorize(Roles = "ShopperAssistant,Customer")]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllCustomersAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.Send(new GetAllCustomersQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(request));
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAsync(long id, UpdateCustomerRequest request)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(id, request));
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserAsync(long id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}