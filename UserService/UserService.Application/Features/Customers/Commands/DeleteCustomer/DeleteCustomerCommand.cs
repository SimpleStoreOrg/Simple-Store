using MediatR;

namespace UserService.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(long CustomerId) : IRequest, IRequest<bool>;