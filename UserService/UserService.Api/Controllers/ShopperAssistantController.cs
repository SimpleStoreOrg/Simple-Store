using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Customers.Commands.CreateCustomer;
using UserService.Application.Features.Customers.Commands.DeleteCustomer;
using UserService.Application.Features.Customers.Commands.UpdateCustomer;
using UserService.Application.Features.Customers.Queries.GetAllCustomers;
using UserService.Application.Features.Customers.Queries.GetCustomerById;
using UserService.Application.Features.ShopperAssistant.Commands.CreateShopperAssistant;
using UserService.Application.Features.ShopperAssistant.Commands.UpdateShopperAssistant;
using UserService.Application.Features.ShopperAssistant.Queries.GetAllShopperAssistants;
using UserService.Application.Features.ShopperAssistant.Queries.GetShopperAssistantById;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopperAssistantController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ShopperAssistantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> ShopperAssistants([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllShopperAssistantsQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShopperAssistantById(long id)
    {
        var result = await _mediator.Send(new GetShopperAssistantByIdQuery(id));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateShopperAssistant(CreateUserRequest request)
    {
        var result = await _mediator.Send(new CreateShopperAssistantCommand(request));
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShopperAssistant(long id, UpdateUserRequest request)
    {
        var result = await _mediator.Send(new UpdateShopperAssistantCommand(id, request));
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}