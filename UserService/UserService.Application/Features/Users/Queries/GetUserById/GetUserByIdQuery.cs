using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(long UserId) : IRequest<UserResponse>;