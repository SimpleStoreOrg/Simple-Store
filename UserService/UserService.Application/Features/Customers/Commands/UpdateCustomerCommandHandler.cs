using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(UpdateCustomerRequest Request) : IRequest<CustomerResponse>;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    public UpdateCustomerCommandHandler(
        IUserServiceDbContext dbContext,
        ILogger<UpdateCustomerCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    
    public async Task<CustomerResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not authorized");
        }

        long customerId = long.Parse(customerIdStr);
        
        _logger.LogInformation("Updating customer with ID: {CustomerId}", customerId);

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken: cancellationToken);
        
        if (customer == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {Id} not found", customerId);
            throw new CustomerNotFoundException(customerId);
        }
        
        var username = request.Request.Username?.Trim().ToLower();
        var email = request.Request.Email?.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber?.Trim().ToLower();

        var exists = await _dbContext.Customers
            .AnyAsync(c => c.Id != customerId && c.Role == RoleStatus.Customer &&
                           (c.UserName!.Trim().ToLower() == username || c.Email!.Trim().ToLower() == email ||
                            c.PhoneNumber!.Trim().ToLower() == phoneNumber),
                cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Customer already exists with email or phone number: {Email},  {PhoneNumber}", email, phoneNumber);
            throw new CustomerAlreadyExistsException();
        }

        customer.Name = request.Request.Name;
        customer.Surname = request.Request.Surname;
        customer.Email = email;
        customer.UserName = request.Request.Username;
        customer.PhoneNumber = phoneNumber;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} updated successfully. Name: {CustomerName}, Surname: {CustomerSurname}", customerId,
            customer.Name, customer.Surname);

        return new CustomerResponse
        {
            Id = customerId,
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