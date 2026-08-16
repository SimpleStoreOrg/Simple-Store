using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.Categories.Queries;

public record GetAllCategoriesQuery(
    int? PageNumber = null,
    int? PageSize = null,
    string? CategoryName = null,
    DateTime? CreatedAtFrom = null,
    DateTime? CreatedAtTo = null,
    long[]? ParentCategoryIds = null) : IRequest<PagedResponse<CategoryResponse>>;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, PagedResponse<CategoryResponse>>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<GetAllCategoriesQueryHandler> _logger;

    public GetAllCategoriesQueryHandler(IProductServiceDbContext dbContext,
        ILogger<GetAllCategoriesQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PagedResponse<CategoryResponse>> Handle(GetAllCategoriesQuery request,
        CancellationToken cancellationToken)

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
        
        var query = _dbContext.Categories.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.CategoryName))
        {
            query = query.Where(c => c.Name.Trim().ToLower() == request.CategoryName.Trim().ToLower());
        }

        if (request.CreatedAtFrom.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= request.CreatedAtFrom);
        }

        if (request.CreatedAtTo.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= request.CreatedAtTo);
        }

        if (request.ParentCategoryIds != null && request.ParentCategoryIds.Length > 0)
        {
            query = query.Where(c =>
                c.ParentCategoryId.HasValue && request.ParentCategoryIds.Contains(c.ParentCategoryId.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var categories = await query
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt
            }).ToListAsync(cancellationToken);

        _logger.LogInformation("Returned {Count} categories out of {Total}", categories.Count, totalCount);

        return new PagedResponse<CategoryResponse>
        {
            Items = categories,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}