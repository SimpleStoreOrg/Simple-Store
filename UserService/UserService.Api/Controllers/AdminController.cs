using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Admins.Commands;
using UserService.Application.Features.Admins.Queries;

namespace UserService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllAdminsAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.Send(new GetAllAdminsQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("marketadmin")]
    public async Task<IActionResult> CreateMarketAdminAsync(CreateAdminRequest request)
    {
        var result = await _mediator.Send(new CreateAdminCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("superadmin")]
    public async Task<IActionResult> CreateSuperAdminAsync(CreateAdminRequest request)
    {
        var result = await _mediator.Send(new CreateSuperAdminCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "SuperAdmin,MarketAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdminAsync(long id, UpdateAdminRequest request)
    {
        var result = await _mediator.Send(new UpdateAdminCommand(id, request));
        return Ok(result);
    }
    
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdminAsync(long id)
    {
        await _mediator.Send(new DeleteAdminCommand(id));
        return NoContent();
    }
}