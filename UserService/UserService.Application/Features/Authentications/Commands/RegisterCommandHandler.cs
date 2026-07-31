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

public record RegisterCommand(RegisterRequest Request) : IRequest<bool>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
{
    private readonly IUserServiceDbContext _context;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IUserServiceDbContext context, ILogger<RegisterCommandHandler> logger,
        JwtService jwtService)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering user: {Username}", request.Request.Username);

        var exists = await _context.Users.AnyAsync(
            u => u.UserName!.Trim().ToLower() == request.Request.Username!.Trim().ToLower()
            && u.Role == request.Request.Role,
            cancellationToken: cancellationToken);

        if (exists)
        {
            _logger.LogInformation("User already exists with this UserName: {Username}", request.Request.Username);
            throw new UserAlreadyExistsException();
        }

        UserEntity user;
        switch (request.Request.Role)
        {
            case RoleStatus.ShopperAssistant:

                user = new ShopperAssistantEntity
                {
                    UserName = request.Request.Username,
                    Email = request.Request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
                    Role = request.Request.Role,
                    CreatedAt = DateTime.UtcNow.AddHours(5)
                };
                break;
            
            case RoleStatus.Customer:
                
                user = new CustomerEntity
                {
                    UserName = request.Request.Username,
                    Email = request.Request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
                    Role = RoleStatus.Customer,
                    CreatedAt = DateTime.UtcNow.AddHours(5)
                };
                break;
            
            default:
                throw new InvalidOperationException("Invalid role");
        }

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User created successfully: {Username}", request.Request.Username);

        return true;
    }
}