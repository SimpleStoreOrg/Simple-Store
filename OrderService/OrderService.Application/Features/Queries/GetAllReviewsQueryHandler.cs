using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Queries;

public record GetAllReviewsQuery(
    int? PageNumber = null,
    int? PageSize = null,
    DateTime? ReviewsFrom = null,
    DateTime? ReviewsTo = null) : IRequest<PagedResponse<ReviewProductResponse>>;
    
public class GetAllReviewsQueryHandler : IRequestHandler<GetAllReviewsQuery, PagedResponse<ReviewProductResponse>>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetAllReviewsQueryHandler> _logger;

    public GetAllReviewsQueryHandler(IOrderServiceDbContext dbContext, ILogger<GetAllReviewsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<PagedResponse<ReviewProductResponse>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Reviews. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
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
        
        var query = _dbContext.Reviews.AsQueryable();

        if (request.ReviewsFrom.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= request.ReviewsFrom);
        }

        if (request.ReviewsTo.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= request.ReviewsTo);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query.OrderBy(r => r.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var reviews = await query
            .Select(r => new ReviewProductResponse
            {
                Id = r.Id,
                OrderId = r.OrderId,
                CustomerId = r.CustomerId,
                ProductId = r.ProductId, 
                Rating = r.Rating,
                Message = r.Message,
                CreatedAt = r.CreatedAt
            }).ToListAsync(cancellationToken);
        return new PagedResponse<ReviewProductResponse>
        {
            Items = reviews,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}