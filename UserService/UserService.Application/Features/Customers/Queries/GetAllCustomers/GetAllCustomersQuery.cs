using MediatR;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<UserResponse>>;