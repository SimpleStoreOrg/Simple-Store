using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Features.Queries;

namespace OrderService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ShopperAssistant, Customer")]
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

    [HttpGet("topproductsbycategory")]
    public async Task<IActionResult> GetTopProductsByCategoryAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)

    {
        var result = await _mediator.Send(new GetTopProductsByCategoryQuery(pageNumber, pageSize));
        return Ok(result);
    }
}