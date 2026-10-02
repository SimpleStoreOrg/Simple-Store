using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Request;
using OrderService.Application.Features.Carts.Commands;
using OrderService.Application.Features.Carts.Queries;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet]
    public async Task<IActionResult> GetAllCartsAsync([FromQuery]int? pageNumber, [FromQuery]int? pageSize)
    {
        var result = await _mediator.Send(new GetAllCartsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateCartItemAsync(long productId, UpdateCartItemRequest request)
    {
        var result = await _mediator.Send(new UpdateCartItemCommand(productId, request));
        return Ok(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my-cart")]
    public async Task<IActionResult> GetMyCartAsync()
    {
        var result = await _mediator.Send(new GetMyCartQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateCartAsync()
    {
        var result = await _mediator.Send(new CreateCartCommand());
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPost("additemtocart")]
    public async Task<IActionResult> AddItemToCartAsync(CreateCartRequest request)
    {
        var result = await _mediator.Send(new AddItemToCartCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveCartItemAsync(long productId)
    {
        var result = await _mediator.Send(new RemoveCartItemCommand(productId));
        return Ok(result);
    }
}