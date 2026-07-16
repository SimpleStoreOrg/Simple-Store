using MediatR;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(long UserId, UpdateUserRequest Request) : IRequest<UserResponse>;