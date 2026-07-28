using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Users.Queries;

namespace UserService.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetAllUSers([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(pageNumber, pageSize));
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(long id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        return Ok(result);
    }
}