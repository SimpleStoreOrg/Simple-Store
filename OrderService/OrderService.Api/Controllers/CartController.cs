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

    [Authorize(Roles = "Customer,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCartAsync(CreateCartRequest request)
    {
        var result = await _mediator.Send(new CreateCartCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer,Admin")]
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckoutAsync()
    {
        var result = await _mediator.Send(new CheckoutCartCommand());
        return Ok(result);
    }
}