using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Features.Commands;
using OrderService.Application.Features.Queries;

namespace OrderService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllOrdersQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(long id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var result = await _mediator.Send(new CreateOrderCommand(request));
        return Ok(result);
    }
    
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(long id)
    {
        await _mediator.Send(new CancelOrderCommand(id));
        return NoContent();
    }
    
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayAsync(int id, PayOrderRequest request)
    {
        var result = await _mediator.Send(new PayOrderCommand(id, request.AmountPaid));
        return Ok(result);
    }
}