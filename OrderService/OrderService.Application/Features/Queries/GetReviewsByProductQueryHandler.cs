using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Queries;

public record GetReviewsByProductQuery(long ProductId) : IRequest<ReviewsByProductSummaryResponse>;

public class GetReviewsByProductQueryHandler : IRequestHandler<GetReviewsByProductQuery, ReviewsByProductSummaryResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetReviewsByProductQueryHandler> _logger;

    public GetReviewsByProductQueryHandler(IOrderServiceDbContext dbContext,
        ILogger<GetReviewsByProductQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ReviewsByProductSummaryResponse> Handle(GetReviewsByProductQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching reviews for product {ProductId}", request.ProductId);

        var reviews = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewProductResponse
            {
                Id = r.Id,
                OrderId = r.OrderId,
                CustomerId = r.CustomerId,
                ProductId = r.ProductId,
                MarketId = r.MarketId,
                Rating = r.Rating,
                Message = r.Message,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var reviewCount = reviews.Count;

        double averageRating;
        if (reviewCount == 0)
        {
            averageRating = 0d;
        }
        else
        {
            averageRating = Math.Round(reviews.Average(r => r.Rating), 2);
        }

        return new ReviewsByProductSummaryResponse
        {
            ProductId = request.ProductId,
            AverageRating = averageRating,
            ReviewCount = reviewCount,
            Reviews = reviews
        };
    }
}