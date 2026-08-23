using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Queries;
public record GetTotalRevenueQuery(DateTime? From = null, DateTime? To = null) : IRequest<TotalRevenueResponse>;

public class GetTotalRevenueQueryHandler : IRequestHandler<GetTotalRevenueQuery, TotalRevenueResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetTotalRevenueQueryHandler> _logger;

    public GetTotalRevenueQueryHandler(IOrderServiceDbContext dbContext, ILogger<GetTotalRevenueQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<TotalRevenueResponse> Handle(GetTotalRevenueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Total orders and revenue command execution");
        
        var query = _dbContext.Orders.AsQueryable();
        
        _logger.LogInformation("Looking for Completed orders");
        query = query.Where(o => o.Status == OrderStatus.Completed);
        
        if (!request.From.HasValue && !request.To.HasValue)
        {
            query = query.Where(o =>
                o.CreatedAt >= DateTime.UtcNow.Date && o.CreatedAt <= DateTime.UtcNow.AddHours(5));
        }
        
        if (request.From.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.From);
        }

        if (request.To.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.To);
        }

        var totalOrders = await query.CountAsync(cancellationToken);

        var totalRevenue = await query
            .SelectMany(o => o.OrderItems)
            .SumAsync(o => o.Price * o.Quantity, cancellationToken);

        return new TotalRevenueResponse
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue
        };
    }
}