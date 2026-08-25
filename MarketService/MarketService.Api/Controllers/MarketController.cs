using MarketService_Application.DTOs.Request;
using MarketService_Application.Features.Commands;
using MarketService_Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Customer,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetAllMarketsAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize,
        [FromQuery] string? marketName)
    {
        var result = await _mediator.Send(new GetAllMarketsQuery(pageNumber, pageSize, marketName));
        return Ok(result);
    }
    
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateMarketAsync(CreateMarketRequest request)
    {
        var result = await _mediator.Send(new CreateMarketCommand(request));
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,MarketAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMarketAsync(long id, UpdateMarketRequest request)
    {
        var result = await _mediator.Send(new UpdateMarketCommand(id, request));
        return Ok(result);
    }
    
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMarketAsync(long id)
    {
        var result = await _mediator.Send(new DeleteMarketCommand(id));
        return Ok(result);
    }
}