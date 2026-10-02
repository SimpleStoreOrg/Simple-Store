using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.Features.Authentications.Commands;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request));
        return Ok(result);
    }
    
    [HttpPost("registercustomer")]
    public async Task<IActionResult> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        await _mediator.Send(new RegisterCustomerCommand(request));
        return NoContent();
    }
}