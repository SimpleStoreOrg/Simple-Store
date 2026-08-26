using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Features.Queries;

namespace OrderService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("totalrevenue")]
    public async Task<IActionResult> GetTotalRevenueAsync(DateTime? from, DateTime? to)
    {
        var result = await _mediator.Send(new GetTotalRevenueQuery(from, to));
        return Ok(result);
    }

    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("topproductsbycategory")]
    public async Task<IActionResult> GetTopProductsByCategoryAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] long[]? categoryIds)

    {
        var result = await _mediator
            .Send(new GetTopProductsByCategoryQuery(pageNumber, pageSize, categoryIds));
        return Ok(result);
    }

    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("reviewedproducts")]
    public async Task<IActionResult> GetAllReviewsAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize,
        [FromQuery] DateTime? reviewsFrom, [FromQuery] DateTime? reviewsTo)

    {
        var result = await _mediator.Send(new GetAllReviewsQuery(
            pageNumber, pageSize, reviewsFrom, reviewsTo));
        return Ok(result);
    }
}