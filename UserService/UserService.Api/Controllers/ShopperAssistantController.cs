using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Features.ShopperAssistants.Commands;
using UserService.Application.Features.ShopperAssistants.Queries;
using UserService.Domain.Enums;


namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopperAssistantController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ShopperAssistantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ShopperAssistantResponse>> GetAllShopperAssistantsAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] ShopperAssistantPosition? positions)
    {
        var result = await _mediator.Send(new GetAllShopperAssistantsQuery(
            pageNumber, pageSize, positions));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentShopperAssistantAsync()
    {
        var shopperAssistantStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (shopperAssistantStr == null)
        {
            throw new NotAuthorizedException("ShopperAssistant is not Authorized");
        }

        long shopperAssistantId = long.Parse(shopperAssistantStr);

        var result = await _mediator.Send(new GetShopperAssistantByIdQuery(shopperAssistantId));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin,ShopperAssistant")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShopperAssistantByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetShopperAssistantByIdQuery(id));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateShopperAssistantAsync(CreateShopperAssistantRequest request)
    {
        var result = await _mediator.Send(new CreateShopperAssistantCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShopperAssistantAsync(long id, UpdateShopperAssistantRequest request)
    {
        var result = await _mediator.Send(new UpdateShopperAssistantCommand(id, request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShopperAssistantAsync(long id)
    {
        await _mediator.Send(new DeleteShopperAssistantCommand(id));
        return NoContent();
    }
}