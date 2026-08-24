using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Authentications.Commands;

public record RegisterCustomerCommand(RegisterCustomerRequest Request) : IRequest<bool>;

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, bool>
{
    private readonly IUserServiceDbContext _context;
    private readonly ILogger<RegisterCustomerCommandHandler> _logger;

    public RegisterCustomerCommandHandler(IUserServiceDbContext context, ILogger<RegisterCustomerCommandHandler> logger,
        JwtService jwtService)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<bool> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering Customer: {Username}", request.Request.Username);

        var exists = await _context.Customers.AnyAsync(
            u => u.UserName!.Trim().ToLower() == request.Request.Username!.Trim().ToLower(),
            cancellationToken: cancellationToken);

        if (exists)
        {
            _logger.LogInformation("Customer already exists with this Username: {Username}", request.Request.Username);
            throw new UserAlreadyExistsException();
        }
                
        var customer = new CustomerEntity
        {
            UserName = request.Request.Username,
            Email = request.Request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            Role = RoleStatus.Customer
        };

        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Customer created successfully: {Username}", request.Request.Username);

        return true;
    }
}