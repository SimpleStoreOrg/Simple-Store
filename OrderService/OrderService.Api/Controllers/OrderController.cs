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
[Authorize(Roles = "ShopperAssistant,Customer")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrderByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(CreateOrderRequest request)
    {
        var result = await _mediator.Send(new CreateOrderCommand(request));
        return Ok(result);
    }

    [HttpPost("{id}/assign-assistant")]
    public async Task<IActionResult> AssignOrderAsync(long id, AssignOrderRequest request)
    {
        await _mediator.Send(new AssignOrderCommand(id, request));
        return Ok();
    }

    [HttpPost("{id}/update-orderstatus")]
    public async Task<IActionResult> ChangeOrderStatusAsync(long id, UpdateOrderStatusRequest request)
    {
        await _mediator.Send(new UpdateOrderStatusCommand(id, request));
        return Ok();
    }
    
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayAsync(int id, PayOrderRequest request)
    {
        var result = await _mediator.Send(new PayOrderCommand(id, request.AmountPaid));
        return Ok(result);
    }

    [HttpPost("reviewproduct")]
    public async Task<IActionResult> ReviewProductAsync(ReviewProductRequest request)
    {
        await _mediator.Send(new ReviewProductCommand(request));
        return NoContent();
    }
}