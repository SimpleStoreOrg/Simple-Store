using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(long CustomerId ,UpdateCustomerRequest Request) : IRequest<CustomerResponse>;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(IUserServiceDbContext dbContext, ILogger<UpdateCustomerCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<CustomerResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", request.CustomerId);

        var username = request.Request.Username!.Trim().ToLower();
        var email = request.Request.Email!.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber!.Trim().ToLower();

        var exists = await _dbContext.Customers
            .AnyAsync(c =>
                    (c.Id != request.CustomerId) &&
                    (c.UserName!.Trim().ToLower() == username || c.Email!.Trim().ToLower() == email ||
                     c.PhoneNumber!.Trim().ToLower() == phoneNumber),
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
        customer.UserName = request.Request.Username;
        customer.PhoneNumber = phoneNumber;
        customer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} updated successfully. Name: {CustomerName}, Surname: {CustomerSurname}", request.CustomerId,
            customer.Name, customer.Surname);

        return new CustomerResponse
        {
            Id = request.CustomerId,
            Name = customer.Name,
            Surname = customer.Surname,
            Role = customer.Role,
            Username = customer.UserName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}