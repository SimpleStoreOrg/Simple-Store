using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Queries;
public record GetTotalRevenueQuery(DateTime? From = null, DateTime? To = null) : IRequest<TotalRevenueResponse>;

public class GetTotalRevenueQueryHandler : IRequestHandler<GetTotalRevenueQuery, TotalRevenueResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetTotalRevenueQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetTotalRevenueQueryHandler(
        IOrderServiceDbContext dbContext, 
        ILogger<GetTotalRevenueQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<TotalRevenueResponse> Handle(GetTotalRevenueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Total orders and revenue command execution");
        
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;
        
        var query = _dbContext.Orders.AsQueryable();
        
        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var marketIdClaim = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdClaim == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdClaim);

            query = query.Where(o => o.OrderItems.Any(oi => oi.MarketId == marketId));
        }
        
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