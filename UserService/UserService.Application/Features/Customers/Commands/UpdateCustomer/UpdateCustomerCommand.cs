using MediatR;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(long CustomerId ,UpdateUserRequest Request) : IRequest<UserResponse>;