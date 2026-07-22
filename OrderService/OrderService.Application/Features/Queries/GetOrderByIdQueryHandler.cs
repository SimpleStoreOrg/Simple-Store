using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Queries;

public record GetOrderByIdQuery(long OrderId) : IRequest<OrderResponse>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(IOrderServiceDbContext dbContext, ILogger<GetOrderByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.Include(oi => oi.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        
        if (order == null)
        {
            _logger.LogWarning("Order with ID {Id} not found", request.OrderId);

            throw new Exception("Order not found");
        }
        
        return new OrderResponse()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ShopperAssistantId = order.ShopperAssistantId,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            DeletedAt = order.DeletedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponse()
            {
                ProductId = oi.ProductId,
                Price = oi.Price,
                Quantity = oi.Quantity,
                TotalPrice = oi.Quantity * oi.Price
            }).ToList(),
            PickUpDeadline = order.PickUpDeadline
        };
    }
}