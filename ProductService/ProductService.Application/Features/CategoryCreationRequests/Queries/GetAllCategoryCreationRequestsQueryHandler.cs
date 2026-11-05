using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;
using ProductService.Domain.Enums;

namespace ProductService.Application.Features.CategoryCreationRequests.Queries;

public record GetAllCategoryCreationRequestsQuery(
    int? PageNumber = null,
    int? PageSize = null,
    CategoryRequestStatus? Status = null,
    DateTime? CreatedAtFrom = null,
    DateTime? CreatedAtTo = null) : IRequest<PagedResponse<CreateCategoryRequestResponse>>;

public class GetAllCategoryCreationRequestsQueryHandler : IRequestHandler<GetAllCategoryCreationRequestsQuery,
    PagedResponse<CreateCategoryRequestResponse>>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<GetAllCategoryCreationRequestsQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetAllCategoryCreationRequestsQueryHandler(
        IProductServiceDbContext dbContext,
        ILogger<GetAllCategoryCreationRequestsQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task<PagedResponse<CreateCategoryRequestResponse>> Handle(GetAllCategoryCreationRequestsQuery request,
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
        
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;
        
        var query = _dbContext.CategoryCreationRequests.AsNoTracking().AsQueryable();

        if (role == "Admin" && adminPosition == "MarketAdmin")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdStr);
            
            query = query.Where(c => c.MarketId == marketId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        if (request.CreatedAtFrom.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= request.CreatedAtFrom);
        }

        if (request.CreatedAtTo.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= request.CreatedAtTo);
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
            .OrderBy(c=>c.Id)
            .Select(c => new CreateCategoryRequestResponse
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                MarketId = c.MarketId,
                MarketAdminId = c.MarketAdminId,
                Status = c.Status,
                ReviewedByAdminId = c.ReviewedByAdminId,
                ReviewedAt = c.ReviewedAt,
                RejectionReason = c.RejectionReason,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt
            }).ToListAsync(cancellationToken);

        _logger.LogInformation("Returned {Count} categories out of {Total}", categories.Count, totalCount);

        return new PagedResponse<CreateCategoryRequestResponse>
        {
            Items = categories,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}