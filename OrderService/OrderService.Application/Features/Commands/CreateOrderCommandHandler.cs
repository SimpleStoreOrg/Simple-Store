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

public record CreateOrderCommand : IRequest<OrderResponse>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IProductApi _productApi;
    private readonly IHttpContextAccessor _accessor;

    public CreateOrderCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CreateOrderCommandHandler> logger,
        IProductApi productApi,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _productApi = productApi;
        _accessor = accessor;
    }
    
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (customerIdStr == null)
        {
            _logger.LogWarning("No Customer Authorized. {CustomerIdStr}", customerIdStr);
            throw new NotAuthorizedException("Customer is not authorized");
        }
        
        long customerId = long.Parse(customerIdStr);
        
        var cart = await _dbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (cart == null)
        {
            throw new CartNotFoundException("Cart not found");
        }

        if (cart.CartItems.Count == 0)
        {
            throw new CartIsEmptyException();
        }
        
        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
                
        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToArray();
                
        var productResponse = await _productApi.GetAllProducts(productIds, token);
                
        var products = productResponse.Items;

        foreach (var cartItem in cart.CartItems)
        {
            var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);

            if (product == null)
            {
                throw new ProductNotFoundException(cartItem.ProductId);
            }
            
            if (cartItem.Quantity > product.Stock)
            {
                _logger.LogWarning(
                    "Insufficient stock for product {ProductId}. Available: {Stock}, Requested: {Quantity}",
                    cartItem.ProductId,
                    product.Stock,
                    cartItem.Quantity);

                throw new InsufficientStockException(
                    cartItem.ProductId,
                    product.Stock,
                    cartItem.Quantity);
            }
        }
                
        _logger.LogInformation("Creating order using Customer ID: {CustomerId}", customerId);

        var order = new OrderEntity
        {
            CustomerId = customerId,
            Status = OrderStatus.New,
            OrderItems = new List<OrderItemsEntity>(),
            PickUpDeadline = DateTime.UtcNow.AddHours(1)
        };
        
        
        
        foreach (var cartItem in cart.CartItems)
        {
            order.OrderItems.Add(new OrderItemsEntity
            {
                Order = order,
                ProductId = cartItem.ProductId,
                MarketId = cartItem.MarketId,
                Price = cartItem.Price,
                Quantity = cartItem.Quantity,
                TotalItemPrice = cartItem.Price * cartItem.Quantity
            });
        }
        order.TotalPrice = cart.TotalPrice;
            
        _dbContext.Carts.Remove(cart);
        
        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
            
        _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
        
        foreach (var item in order.OrderItems)
        {
            await _productApi.UpdateStock(item.ProductId, new UpdateStockRequest()
            {
                Quantity = item.Quantity
            }, token);
        }

        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Items = order.OrderItems
                .Select(ci => new OrderItemResponse
                {
                    MarketId = ci.MarketId,
                    ProductId = ci.ProductId,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    TotalItemPrice = ci.Quantity * ci.Price
                }).ToList(),
            TotalPrice = order.TotalPrice,
            PickUpDeadline = order.PickUpDeadline,
            CreatedAt = order.CreatedAt
        };
    }
}