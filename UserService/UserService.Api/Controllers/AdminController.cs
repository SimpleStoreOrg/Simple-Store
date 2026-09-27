using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
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
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<AdminResponse>> GetAllAdminsAsync(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.Send(new GetAllAdminsQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentAdminAsync()
    {
        var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (adminIdStr == null)
        {
            throw new NotAuthorizedException("Admin is not Authorized");
        }

        long adminId = long.Parse(adminIdStr);

        var result = await _mediator.Send(new GetAdminByIdQuery(adminId));
        return Ok(result);
    }
    
    [Authorize(Policy = "SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<AdminResponse>> GetAdminByIdAsync(long id)
    {
        var result = await _mediator.Send(new GetAdminByIdQuery(id));
        return Ok(result);
    }
    
    [Authorize(Policy = "SuperAdmin")]
    [HttpPost("marketadmin")]
    public async Task<IActionResult> CreateMarketAdminAsync(CreateAdminRequest request)
    {
        var result = await _mediator.Send(new CreateAdminCommand(request));
        return Ok(result);
    }
    
    [Authorize(Policy = "SuperAdmin")]
    [HttpPost("superadmin")]
    public async Task<IActionResult> CreateSuperAdminAsync(CreateAdminRequest request)
    {
        var result = await _mediator.Send(new CreateSuperAdminCommand(request));
        return Ok(result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateAdminAsync(UpdateAdminRequest request)
    {
        var result = await _mediator.Send(new UpdateAdminCommand(request));
        return Ok(result);
    }
    
    [Authorize(Policy = "SuperAdmin")]
    [HttpPut("internal/{id}/assignmarket")]
    public async Task<IActionResult> AssignMarketToAdminAsync(long id, AssignMarketRequest request)
    {
        var result = await _mediator.Send(new AssignMarketToAdminCommand(id, request));
        return Ok(result);
    }
    
    [Authorize(Policy = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdminAsync(long id)
    {
        await _mediator.Send(new DeleteAdminCommand(id));
        return NoContent();
    }
}