using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Features.Users.Commands.CreateUser;
using UserService.Application.Features.Users.Commands.DeleteUser;
using UserService.Application.Features.Users.Commands.UpdateUser;
using UserService.Application.Features.Users.Queries.GetAllUsers;
using UserService.Application.Features.Users.Queries.GetUserById;

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
    
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
    {
        var result = await _mediator.Send(new CreateUserCommand(request));
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(long id, UpdateUserRequest request)
    {
        var result = await _mediator.Send(new UpdateUserCommand(id, request));
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}