using MediatR;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<UserResponse>>;