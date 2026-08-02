using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Customers.Queries;

public record GetCustomerByIdQuery(long CustomerId) : IRequest<CustomerResponse>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandler(IUserServiceDbContext dbContext, ILogger<GetCustomerByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<CustomerResponse> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            throw new CustomerNotFoundException(request.CustomerId);
        }

        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Surname = customer.Surname,
            Role = customer.Role,
            Username = customer.UserName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            DeletedAt = customer.DeletedAt
        };
    }
}