using MarketService_Application.Common;
using MarketService_Application.DTOs.Response;
using MarketService_Application.Exceptions;
using MarketService_Application.Interfaces.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketService_Application.Features.Queries;

public record GetAllMarketsQuery(
    int? PageNumber = null,
    int? PageSize = null,
    string? MarketName = null) : IRequest<PagedResponse<MarketResponse>>;

public class GetAllMarketsQueryHandler : IRequestHandler<GetAllMarketsQuery, PagedResponse<MarketResponse>>
{
    private readonly IMarketServiceDbContext _dbContext;
    private readonly ILogger<GetAllMarketsQueryHandler> _logger;

    public GetAllMarketsQueryHandler(IMarketServiceDbContext dbContext, ILogger<GetAllMarketsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<PagedResponse<MarketResponse>> Handle(GetAllMarketsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Categories. Page: {PageNumber}, Size: {PageSize}", request.PageNumber,
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
        
        var query = _dbContext.Markets.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.MarketName))
        {
            query = query.Where(c => c.Name.Trim().ToLower() == request.MarketName.Trim().ToLower());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var markets = await query
            .OrderBy(m=>m.Id)
            .Select(c => new MarketResponse
            {
                Id = c.Id,
                MarketAdminId = c.MarketAdminId,
                Name = c.Name,
                Location = c.Location,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt
            }).ToListAsync(cancellationToken);

        _logger.LogInformation("Returned {Count} markets out of {Total}", markets.Count, totalCount);

        return new PagedResponse<MarketResponse>
        {
            Items = markets,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}