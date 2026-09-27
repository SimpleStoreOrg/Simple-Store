using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Admins.Queries;

public record GetAllAdminsQuery(
    int? PageNumber = null,
    int? PageSize = null) : IRequest<PagedResponse<AdminResponse>>;

public class GetAllAdminsQueryHandler : IRequestHandler<GetAllAdminsQuery, PagedResponse<AdminResponse>>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetAllAdminsQueryHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetAllAdminsQueryHandler(
        IUserServiceDbContext dbContext,
        ILogger<GetAllAdminsQueryHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<PagedResponse<AdminResponse>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Admins. Page: {PageNumber}, Size: {PageSize}", request.PageNumber,
            request.PageSize);
        if (request.PageNumber.HasValue && request.PageNumber.Value <= 0)
        {
            _logger.LogWarning("Page number {PageNumber}, must be greater than 0", request.PageNumber);
            throw new IncorrectPaginationException("Page number must be greater than 0");
        }

        if (request.PageSize.HasValue && request.PageSize.Value <= 0)
        {
            _logger.LogWarning("Page size {PageSize}, must be greater than 0", request.PageSize);
            throw new IncorrectPaginationException("Page size must be greater than 0");
        }

        var user = _httpContextAccessor.HttpContext?.User;

        var adminIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var adminPositionStr = user?.FindFirst("AdminPosition")?.Value;

        if (string.IsNullOrWhiteSpace(adminIdStr))
        {
            _logger.LogWarning("Admin ID claim was not found");
            throw new NotAuthorizedException("Admin is not authorized.");
        }

        long currentAdminId = long.Parse(adminIdStr);

        if (string.IsNullOrWhiteSpace(adminPositionStr))
        {
            _logger.LogWarning("Admin position claim was not found for admin {AdminId}", currentAdminId);
            throw new NotAuthorizedException("Admin position is not available");
        }

        if (!Enum.TryParse<AdminPosition>(adminPositionStr, out var adminPosition))
        {
            _logger.LogWarning("Invalid admin position {AdminPosition} for admin {AdminId}", adminPositionStr, currentAdminId);
            throw new NotAuthorizedException("Invalid admin position");
        }

        var query = _dbContext.Admins.AsNoTracking().AsQueryable();

        if (adminPosition == AdminPosition.SuperAdmin)
        {
            _logger.LogInformation("SuperAdmin {AdminId} is fetching all admins", currentAdminId);
        }
        else if (adminPosition == AdminPosition.MarketAdmin)
        {
            _logger.LogInformation("MarketAdmin {AdminId} is fetching his own admin record.", currentAdminId);

            query = query.Where(a => a.Id == currentAdminId);
        }
        else
        {
            _logger.LogWarning("Admin {AdminId} has unsupported position {Position}.", currentAdminId, adminPosition);
            throw new NotAuthorizedException("Not authorized to access admins");
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.PageNumber.HasValue &&
            request.PageSize.HasValue)
        {
            query = query
                .OrderBy(a => a.Id)
                .Skip(
                    (request.PageNumber.Value - 1) *
                    request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var admins = await query
            .OrderBy(a => a.Id)
            .Select(e => new AdminResponse
            {
                Id = e.Id,
                MarketId = e.MarketId,
                Name = e.Name,
                Surname = e.Surname,
                Role = e.Role,
                Position = e.Position,
                Username = e.UserName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                DeletedAt = e.DeletedAt
            })
            .ToListAsync(cancellationToken);
        
        _logger.LogInformation("Returned {Count} Admins out of {Total} for admin {AdminId}.", admins.Count, totalCount,
            currentAdminId);
        
        return new PagedResponse<AdminResponse>
        {
            Items = admins,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}