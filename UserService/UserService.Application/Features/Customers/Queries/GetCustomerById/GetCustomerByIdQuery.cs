using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(long CustomerId) : IRequest<UserResponse>;