using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.Products.Queries;

public record GetAllProductsQuery(
    int? PageNumber = null,
    int? PageSize = null,
    bool? IsAvailable = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    long[]? CategoryIds = null,
    DateTime? CreatedAtFrom = null,
    DateTime? CreatedAtTo = null) : IRequest<PagedResponse<ProductResponse>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResponse<ProductResponse>>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetAllProductsQueryHandler(IProductServiceDbContext dbContext,
        ILogger<GetAllProductsQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<PagedResponse<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Products. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
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
        
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;
        
        var query = _dbContext.Products.AsNoTracking().AsQueryable();

        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdStr);
            
            query = query.Where(p => p.MarketId == marketId);
        }

        if (request.IsAvailable.HasValue)
        {
            if (request.IsAvailable.Value)
            {
                query = query.Where(p => p.Stock > 0);
            }
            else
            {
                query = query.Where(p => p.Stock == 0);
            }
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice);
        }
        
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice);
        }
        
        if (request.CreatedAtFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= request.CreatedAtFrom);
        }

        if (request.CreatedAtTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= request.CreatedAtTo);
        }

        if (request.CategoryIds != null && request.CategoryIds.Length > 0)
        {
            query = query.Where(p => request.CategoryIds.Contains(p.CategoryId));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(p => p.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var products = await query
            .OrderBy(p=>p.Id)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                MarketId = p.MarketId,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                DeletedAt = p.DeletedAt
            }).ToListAsync(cancellationToken);

        _logger.LogInformation("Returned {Count} products out of {Total}", products.Count, totalCount);
        
        return new PagedResponse<ProductResponse>
        {
            Items = products,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}