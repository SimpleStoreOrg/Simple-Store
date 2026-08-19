using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Features.Queries;

namespace OrderService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet("totalrevenue")]
    public async Task<IActionResult> GetTotalRevenueAsync(DateTime? from, DateTime? to)
    {
        var result = await _mediator.Send(new GetTotalRevenueQuery(from, to));
        return Ok(result);
    }
}