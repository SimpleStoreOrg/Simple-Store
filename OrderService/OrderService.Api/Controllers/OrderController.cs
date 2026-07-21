using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Request;
using OrderService.Application.Features.Commands;

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

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var result = await _mediator.Send(new CreateOrderCommand(request));
        return Ok(result);
    }
}