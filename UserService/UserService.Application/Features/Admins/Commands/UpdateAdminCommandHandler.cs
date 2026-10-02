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

namespace UserService.Application.Features.Admins.Commands;

public record UpdateAdminCommand(UpdateAdminRequest Request)
    : IRequest<AdminResponse>;

public class UpdateAdminCommandHandler : IRequestHandler<UpdateAdminCommand, AdminResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<UpdateAdminCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public UpdateAdminCommandHandler(
        IUserServiceDbContext dbContext,
        ILogger<UpdateAdminCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task<AdminResponse> Handle(UpdateAdminCommand request,
        CancellationToken cancellationToken)

    {
        var adminIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (adminIdStr == null)
        {
            throw new NotAuthorizedException("Admin is not authorized");
        }

        long adminId = long.Parse(adminIdStr);
        
        _logger.LogInformation("Updating Shopper Assistant with ID: {Id}", adminId);

        var admin = await _dbContext.Admins
            .FirstOrDefaultAsync(x => x.Id == adminId, cancellationToken: cancellationToken);
        
        if (admin == null)
        {
            _logger.LogWarning("Admin with ID {Id} not found", admin);
            throw new AdminNotFoundException(adminId);
        }
        
        var username = request.Request.Username?.Trim().ToLower();
        var email = request.Request.Email?.Trim().ToLower();
        var phoneNumber = request.Request.PhoneNumber?.Trim().ToLower();

        var exists = await _dbContext.Admins
            .AnyAsync(e =>
                    e.Id != adminId && e.Role == RoleStatus.Admin &&
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
            "Admin {Id} updated successfully. Name: {Name}, Surname: {Surname}", adminId,
            admin.Name, admin.Surname);

        return new AdminResponse
        {
            Id = adminId,
            MarketId = admin.MarketId,
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