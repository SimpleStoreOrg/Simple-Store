using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Admins.Commands;

public record AssignMarketToAdminCommand(long AdminId, AssignMarketRequest Request) : IRequest<AdminResponse>;

public class AssignMarketToAdminCommandHandler
    : IRequestHandler<AssignMarketToAdminCommand, AdminResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<AssignMarketToAdminCommandHandler> _logger;

    public AssignMarketToAdminCommandHandler(IUserServiceDbContext dbContext,
        ILogger<AssignMarketToAdminCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AdminResponse> Handle(AssignMarketToAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _dbContext.Admins
            .FirstOrDefaultAsync(x => x.Id == request.AdminId, cancellationToken);

        if (admin == null)
        {
            _logger.LogWarning("Admin with ID {AdminId} not found", request.AdminId);
            throw new AdminNotFoundException(request.AdminId);
        }

        if (admin.Position != Domain.Enums.AdminPosition.MarketAdmin)
        {
            throw new InvalidOperationException("Only MarketAdmin can be assigned to a market");
        }

        if (admin.MarketId.HasValue)
        {
            throw new InvalidOperationException(
                $"MarketAdmin with ID {admin.Id} is already assigned to market {admin.MarketId.Value}");
        }

        admin.MarketId = request.Request.MarketId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Market {MarketId} assigned to MarketAdmin {AdminId}", request.Request.MarketId,
            admin.Id);

        return new AdminResponse
        {
            Id = admin.Id,
            MarketId = admin.MarketId,
            Name = admin.Name,
            Surname = admin.Surname,
            Email = admin.Email,
            Role = admin.Role,
            Position = admin.Position,
            Username = admin.UserName,
            PhoneNumber = admin.PhoneNumber,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt,
            DeletedAt = admin.DeletedAt
        };
    }
}