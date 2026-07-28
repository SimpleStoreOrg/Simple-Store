using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(long CustomerId ,UpdateUserRequest Request) : IRequest<UserResponse>;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(IUserServiceDbContext dbContext, ILogger<UpdateCustomerCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<UserResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", request.CustomerId);

        var email = request.Request.Email.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber.Trim().ToLower();

        var exists = await _dbContext.Customers
            .AnyAsync(c =>
                    (c.Id != request.CustomerId) &&
                    (c.Email.Trim().ToLower() == email || c.PhoneNumber.Trim().ToLower() == phoneNumber),
                cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Customer already exists with email or phone number: {Email},  {PhoneNumber}", email, phoneNumber);
            throw new CustomerAlreadyExistsException();
        }
        
        var customer =
            await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            throw new CustomerNotFoundException(request.CustomerId);
        }

        customer.Name = request.Request.Name;
        customer.Surname = request.Request.Surname;
        customer.Email = email;
        customer.PhoneNumber = phoneNumber;
        customer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} updated successfully. Name: {CustomerName}, Surname: {CustomerSurname}", request.CustomerId,
            customer.Name, customer.Surname);

        return new UserResponse()
        {
            Id = request.CustomerId,
            Name = customer.Name,
            Surname = customer.Surname,
            Role = customer.Role,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}