using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record CancelOrderByMarketCommand(long OrderId) : IRequest;

public class CancelOrderByMarketCommandHandler : IRequestHandler<CancelOrderByMarketCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CancelOrderByMarketCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public CancelOrderByMarketCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CancelOrderByMarketCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task Handle(CancelOrderByMarketCommand request, CancellationToken cancellationToken)
    {
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        var query = _dbContext.Orders.Include(o => o.OrderItems)
            .Where(o => o.Id == request.OrderId).AsQueryable();

        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdStr);

            query = query.Where(p => p.OrderItems.Any(oi => oi.MarketId == marketId));
        }
        
        var order = await query.FirstOrDefaultAsync(cancellationToken);
        
        if (order == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new OrderAlreadyPaidException(request.OrderId);
        }

        if (order.Status == OrderStatus.CancelledByShop || order.Status == OrderStatus.CancelledByCustomer)
        {
            throw new InvalidOrderException("Order is cancelled by Shop or Customer");
        }
        
        try
        {
            _logger.LogInformation("Cancelling Order {OrderId}", request.OrderId);
            
            order.Status = OrderStatus.CancelledByShop;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occured while cancelling order for Order ID: {OrderId}",
                request.OrderId);
            throw;
        }
    }
}