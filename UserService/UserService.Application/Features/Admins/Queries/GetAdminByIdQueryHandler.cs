using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Admins.Queries;

public record GetAdminByIdQuery(long AdminId) : IRequest<AdminResponse>;

public class GetAdminByIdQueryHandler : IRequestHandler<GetAdminByIdQuery, AdminResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetAdminByIdQueryHandler> _logger;

    public GetAdminByIdQueryHandler(IUserServiceDbContext dbContext, ILogger<GetAdminByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<AdminResponse> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.Id == request.AdminId, cancellationToken);
        if (admin == null)
        {
            _logger.LogWarning("Admin with ID {Id} not found", request.AdminId);
            throw new AdminNotFoundException(request.AdminId);
        }

        return new AdminResponse
        {
            Id = admin.Id,
            MarketId = admin.MarketId,
            Name = admin.Name,
            Surname = admin.Surname,
            Role = admin.Role,
            Position = admin.Position,
            Username = admin.UserName,
            Email = admin.Email,
            PhoneNumber = admin.PhoneNumber,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt,
            DeletedAt = admin.DeletedAt
        };
    }
}