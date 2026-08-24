using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Admins.Queries;

public record GetAllAdminsQuery(
    int? PageNumber = null,
    int? PageSize = null) : IRequest<PagedResponse<AdminResponse>>;

public class GetAllAdminsQueryHandler : IRequestHandler<GetAllAdminsQuery, PagedResponse<AdminResponse>>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetAllAdminsQueryHandler> _logger;

    public GetAllAdminsQueryHandler(IUserServiceDbContext dbContext, ILogger<GetAllAdminsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<PagedResponse<AdminResponse>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Admins. Page: {PageNumber}, Size: {PageSize}", request.PageNumber,
            request.PageSize);
        if (request.PageNumber.HasValue && request.PageNumber.Value <= 0)
        {
            _logger.LogWarning("Page number {PageNumber}, must be greater than 0", request.PageNumber);
            throw new IncorrectPaginationException("Page number must be greater than 0.");
        }

        if (request.PageSize.HasValue && request.PageSize.Value <= 0)
        {
            _logger.LogWarning("Page size {PageSize}, must be greater than 0", request.PageSize);
            throw new IncorrectPaginationException("Page size must be greater than 0.");
        }

        var query = _dbContext.Admins.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var admins = await query
            .Select(e => new AdminResponse
            {
                Id = e.Id,
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
            }).ToListAsync(cancellationToken);
        
        _logger.LogInformation("Returned {Count} Admins out of {Total}", admins.Count, totalCount);
        
        return new PagedResponse<AdminResponse>
        {
            Items = admins,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}