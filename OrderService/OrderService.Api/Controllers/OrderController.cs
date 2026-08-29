using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Features.Commands;
using OrderService.Application.Features.Queries;
using OrderService.Domain.Enums;

namespace OrderService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrdersAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] long[]? customerIds,
        [FromQuery] long[]? shopperAssistant,
        [FromQuery] OrderStatus? statuses,
        [FromQuery] DateTime? createdAtFrom,
        [FromQuery] DateTime? createdAtTo)
    {
        var result =
            await _mediator.Send(new GetAllOrdersQuery(pageNumber, pageSize, customerIds, shopperAssistant, statuses,
                createdAtFrom, createdAtTo));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrderByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        return Ok(result);
    }

    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpPost("{id}/assign-assistant")]
    public async Task<IActionResult> AssignOrderAsync(long id, AssignOrderRequest request)
    {
        await _mediator.Send(new AssignOrderCommand(id, request));
        return Ok();
    }

    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpPost("{id}/update-orderstatus")]
    public async Task<IActionResult> ChangeOrderStatusAsync(long id, UpdateOrderStatusRequest request)
    {
        await _mediator.Send(new UpdateOrderStatusCommand(id, request));
        return Ok();
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPost("{id}/cancelbycustomer")]
    public async Task<IActionResult> CancelByCustomerAsync(long id)
    {
        await _mediator.Send(new CancelOrderCommand(id));
        return NoContent();
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpPost("{id}/cancelbymarket")]
    public async Task<IActionResult> CancelByMarketAsync(long id)
    {
        await _mediator.Send(new CancelOrderByMarketCommand(id));
        return NoContent();
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayAsync(int id, PayOrderRequest request)
    {
        var result = await _mediator.Send(new PayOrderCommand(id, request.AmountPaid));
        return Ok(result);
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpPost("reviewproduct")]
    public async Task<IActionResult> ReviewProductAsync(ReviewProductRequest request)
    {
        await _mediator.Send(new ReviewProductCommand(request));
        return NoContent();
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("internal/customerids")]
    public async Task<IActionResult> GetMarketCustomerIdsAsync()
    {
        var result = await _mediator.Send(new GetMarketCustomerIdsQuery());
        return Ok(result);
    }
}