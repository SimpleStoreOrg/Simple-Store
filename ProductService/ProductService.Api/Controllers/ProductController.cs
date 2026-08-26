using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Features.Products.Queries;

namespace ProductService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Authorize(Roles = "Customer,Admin,ShopperAssistant")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAllProductsAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] bool? isAvailable,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] long[]? categoryIds,
        [FromQuery] DateTime? createdAtFrom,
        [FromQuery] DateTime? createdAtTo)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(pageNumber, pageSize,
            isAvailable, minPrice, maxPrice, categoryIds, createdAtFrom, createdAtTo));
        return Ok(result);
    }

    [Authorize(Roles = "Customer,Admin,ShopperAssistant")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductByIdAsync(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProductAsync(CreateProductRequest request)
    {
        var result = await _mediator.Send(new CreateProductCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductAsync(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }

    [HttpPut("internal/{id}/stock")]
    public async Task<IActionResult> UpdateStockAsync(long id, UpdateStockRequest request)
    {
        await _mediator.Send(new UpdateStockCommand(id, request.Quantity));
        return Ok();
    }
}