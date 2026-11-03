using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.External;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record ReorderPreviousOrderCommand(long OrderId) : IRequest<OrderResponse>;

public class ReorderPreviousOrderCommandHandler : IRequestHandler<ReorderPreviousOrderCommand, OrderResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<ReorderPreviousOrderCommandHandler> _logger;
    private readonly IProductApi _productApi;
    private readonly IHttpContextAccessor _accessor;

    public ReorderPreviousOrderCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<ReorderPreviousOrderCommandHandler> logger,
        IProductApi productApi,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _productApi = productApi;
        _accessor = accessor;
    }

    public async Task<OrderResponse> Handle(ReorderPreviousOrderCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not authorized");
        }

        long customerId = long.Parse(customerIdStr);

        var original = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (original == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (original.CustomerId != customerId)
        {
            throw new NotAuthorizedException("You cannot reorder another customer's order");
        }

        if (original.Status != OrderStatus.Completed &&
            original.Status != OrderStatus.CancelledByCustomer &&
            original.Status != OrderStatus.CancelledByShop)
        {
            throw new InvalidOrderException("You cannot reorder the current order. Check the status");
        }

        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();

        var productIds = original.OrderItems.Select(oi => oi.ProductId).ToArray();
        var productResponse = await _productApi.GetAllProducts(productIds, token);

        foreach (var item in original.OrderItems)
        {
            var product = productResponse.Items.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
            {
                throw new ProductNotFoundException(item.ProductId);
            }

            if (item.Quantity > product.Stock)
            {
                throw new InsufficientStockException(item.ProductId, product.Stock, item.Quantity);
            }
        }

        _logger.LogInformation("Creating reorder for Customer {CustomerId} from Order {OrderId}", customerId, original.Id);

        var reorder = new OrderEntity
        {
            CustomerId = customerId,
            Status = OrderStatus.New,
            PickUpDeadline = DateTime.UtcNow.AddHours(1),
            OrderItems = new List<OrderItemsEntity>()
        };

        foreach (var item in original.OrderItems)
        {
            reorder.OrderItems.Add(new OrderItemsEntity
            {
                Order = reorder,
                ProductId = item.ProductId,
                MarketId = item.MarketId,
                Price = item.Price,
                Quantity = item.Quantity,
                TotalItemPrice = item.Price * item.Quantity
            });
        }

        reorder.TotalPrice = reorder.OrderItems.Sum(oi => oi.TotalItemPrice);

        await _dbContext.Orders.AddAsync(reorder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var item in reorder.OrderItems)
        {
            await _productApi.UpdateStock(item.ProductId, new UpdateStockRequest
            {
                Quantity = item.Quantity
            }, token);
        }

        return new OrderResponse
        {
            Id = reorder.Id,
            CustomerId = reorder.CustomerId,
            Items = reorder.OrderItems.Select(oi => new OrderItemResponse
            {
                MarketId = oi.MarketId,
                ProductId = oi.ProductId,
                Price = oi.Price,
                Quantity = oi.Quantity,
                TotalItemPrice = oi.TotalItemPrice
            }).ToList(),
            TotalPrice = reorder.TotalPrice,
            PickUpDeadline = reorder.PickUpDeadline,
            CreatedAt = reorder.CreatedAt
        };
    }
}
