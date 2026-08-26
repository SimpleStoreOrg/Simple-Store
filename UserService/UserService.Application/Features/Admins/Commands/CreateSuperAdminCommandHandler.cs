using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Admins.Commands;

public record CreateSuperAdminCommand(CreateAdminRequest Request) : IRequest<AdminResponse>;


public class CreateSuperAdminCommandHandler : IRequestHandler<CreateSuperAdminCommand, AdminResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<CreateSuperAdminCommandHandler> _logger;

    public CreateSuperAdminCommandHandler(IUserServiceDbContext dbContext, ILogger<CreateSuperAdminCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<AdminResponse> Handle(CreateSuperAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Super Admin. Name: {AdminName}, Surname: {AdminSurname}", request.Request.Name,
            request.Request.Surname);

        var username = request.Request.Username!.Trim().ToLower();
        var email = request.Request.Email!.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber!.Trim().ToLower();

        var exists =
            await _dbContext.Admins.AnyAsync(
                e => e.UserName!.Trim().ToLower() == username || e.Email!.Trim().ToLower() == email ||
                     e.PhoneNumber!.Trim().ToLower() == phoneNumber,
                cancellationToken);
        
        if (exists)
        {
            _logger.LogWarning("Super Admin already exists with email or phone number: {Email}, {PhoneNumber}", email,
                phoneNumber);
            throw new AdminAlreadyExistsException();
        }
        
        var admin = new AdminEntity
        {
            Name = request.Request.Name,
            Surname = request.Request.Surname,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            Role = RoleStatus.Admin,
            Position = AdminPosition.SuperAdmin,
            Email = email,
            UserName = request.Request.Username,
            PhoneNumber = phoneNumber
        };

        await _dbContext.Admins.AddAsync(admin, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Super Admin created successfully with ID: {AdminId}", admin.Id);

        return new AdminResponse
        {
            Id = admin.Id,
            Name = admin.Name,
            Surname = admin.Surname,
            Role = admin.Role,
            Position = admin.Position,
            Username = admin.UserName,
            Email = admin.Email,
            PhoneNumber = admin.PhoneNumber,
            CreatedAt = admin.CreatedAt
        };
    }
}