using MediatR;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(CreateUserRequest Request) : IRequest<UserResponse>;