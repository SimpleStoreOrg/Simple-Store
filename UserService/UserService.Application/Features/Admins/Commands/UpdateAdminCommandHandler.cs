using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Admins.Commands;

public record UpdateAdminCommand(long AdminId, UpdateAdminRequest Request)
    : IRequest<AdminResponse>;

public class UpdateAdminCommandHandler : IRequestHandler<UpdateAdminCommand, AdminResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateAdminCommandHandler> _logger;

    public UpdateAdminCommandHandler(IUserServiceDbContext dbContext,
        ILogger<UpdateAdminCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AdminResponse> Handle(UpdateAdminCommand request,
        CancellationToken cancellationToken)

    {
        _logger.LogInformation("Updating Shopper Assistant with ID: {Id}", request.AdminId);

        var admin = await _dbContext.Admins
            .FirstOrDefaultAsync(x => x.Id == request.AdminId, cancellationToken: cancellationToken);
        
        if (admin == null)
        {
            _logger.LogWarning("Admin with ID {Id} not found", request.AdminId);
            throw new AdminNotFoundException(request.AdminId);
        }
        
        var username = request.Request.Username?.Trim().ToLower();
        var email = request.Request.Email?.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber?.Trim().ToLower();

        var exists = await _dbContext.Admins
            .AnyAsync(e =>
                    e.Id != request.AdminId && e.Role == RoleStatus.Admin &&
                    (e.UserName!.Trim().ToLower() == username || e.Email!.Trim().ToLower() == email ||
                     e.PhoneNumber!.Trim().ToLower() == phoneNumber),
                cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Admin already exists with username or email or phone number: {Email}, {PhoneNumber}", email,
                phoneNumber);
            throw new AdminAlreadyExistsException();
        }
        
        admin.Name = request.Request.Name;
        admin.Surname = request.Request.Surname;
        admin.Email = email;
        admin.UserName = request.Request.Username;
        admin.PhoneNumber = phoneNumber;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {Id} updated successfully. Name: {Name}, Surname: {Surname}", request.AdminId,
            admin.Name, admin.Surname);

        return new AdminResponse
        {
            Id = request.AdminId,
            Name = admin.Name,
            Surname = admin.Surname,
            Email = admin.Email,
            Role = admin.Role,
            Position = admin.Position,
            Username = admin.UserName,
            PhoneNumber = admin.PhoneNumber,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt
        };
    }
}