using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Customers.Commands;

public record CreateCustomerCommand(CreateUserRequest Request) : IRequest<UserResponse>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(IUserServiceDbContext dbContext, ILogger<CreateCustomerCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<UserResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer. Name: {CustomerName}, Surname: {CustomerSurname}", request.Request.Name,
            request.Request.Surname);
        
        var email = request.Request.Email.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber.Trim().ToLower();

        var exists =
            await _dbContext.Customers.AnyAsync(
                e => e.Email.Trim().ToLower() == email || e.PhoneNumber.Trim().ToLower() == phoneNumber,
                cancellationToken);
        
        if (exists)
        {
            _logger.LogWarning("Customer already exists with email or phone number: {Email}, {PhoneNumber}", email,
                phoneNumber);
            throw new CustomerAlreadyExistsException();
        }
        
        var customer = new CustomerEntity()
        {
            Name = request.Request.Name,
            Surname = request.Request.Surname,
            Role = request.Request.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

        return new UserResponse()
        {
            Id = customer.Id,
            Name = customer.Name,
            Surname = customer.Surname,
            Role = customer.Role,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt
        };
    }
}