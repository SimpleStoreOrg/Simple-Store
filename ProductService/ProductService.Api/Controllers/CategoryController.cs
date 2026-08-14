using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Features.Categories.Commands;
using ProductService.Application.Features.Categories.Queries;

namespace ProductService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ShopperAssistant,Customer")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CategoryResponse>>> GetAllCategoriesAsync(
        [FromQuery] string? categoryName,
        [FromQuery] DateTime? createdAtFrom,
        [FromQuery] DateTime? createdAtTo,
        [FromQuery] long?[]? parentCategoryIds,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result =
            await _mediator.Send(new GetAllCategoriesQuery(pageNumber, pageSize, categoryName, createdAtFrom,
                createdAtTo, parentCategoryIds));
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryByIdAsync(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(request));
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, request));
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoryAsync(int id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}