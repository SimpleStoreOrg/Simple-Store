using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Queries;

public record GetOrderByIdQuery(long OrderId) : IRequest<OrderResponse>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetOrderByIdQueryHandler(
        IOrderServiceDbContext dbContext,
        ILogger<GetOrderByIdQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    
    public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;
        
        var query = _dbContext.Orders.Include(o => o.OrderItems)
            .Where(o => o.Id == request.OrderId);
        
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

        var order = await query.FirstOrDefaultAsync(cancellationToken);
        
        if (order == null)
        {
            _logger.LogWarning("Order with ID {Id} not found", request.OrderId);
            throw new OrderNotFoundException(request.OrderId);
        }
        
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ShopperAssistantId = order.ShopperAssistantId,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            DeletedAt = order.DeletedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponse
            {
                MarketId = oi.MarketId,
                ProductId = oi.ProductId,
                Price = oi.Price,
                Quantity = oi.Quantity,
                TotalPrice = oi.Quantity * oi.Price
            }).ToList(),
            PickUpDeadline = order.PickUpDeadline
        };
    }
}