using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Application.Features.Products.Commands.DeleteProduct;
using ProductService.Application.Features.Products.Commands.UpdateProduct;
using ProductService.Application.Features.Products.Queries.GetAllProducts;
using ProductService.Application.Features.Products.Queries.GetProductById;

namespace ProductService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAllProducts([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        var result = await _mediator.Send(new CreateProductCommand(request));
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, request));
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}