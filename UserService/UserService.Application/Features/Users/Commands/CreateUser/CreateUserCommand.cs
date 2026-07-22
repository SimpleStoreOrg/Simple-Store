using MediatR;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserResponse>;