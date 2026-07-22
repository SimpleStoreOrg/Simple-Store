using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Refit;

namespace OrderService.Application.Features.Commands;

public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<OrderResponse>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IProductApi _productApi;
    private readonly IUserApi _userApi;

    public CreateOrderCommandHandler(IOrderServiceDbContext dbContext, ILogger<CreateOrderCommandHandler> logger,
        IProductApi productApi, IUserApi userApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _productApi = productApi;
        _userApi = userApi;
    }
    
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Request.Items == null || request.Request.Items.Count <= 0)
        {
            throw new InvalidOrderException("Order must contain at least one item");
        }

        var hasDuplicates = request.Request.Items
            .GroupBy(oi => oi.ProductId)
            .Any(g => g.Count() > 1);

        if (hasDuplicates)
        {
            throw new DuplicateProductException();
        }
        
        var customer = await _userApi.GetUserById(request.Request.CustomerId);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", request.Request.CustomerId);
            throw new CustomerNotFoundException(request.Request.CustomerId);
        }

        var shopperAssistant = await _userApi.GetUserById(request.Request.ShopperAssistantId);

        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {EmployeeId} not found", request.Request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.Request.ShopperAssistantId);
        }

        try
        {
            _logger.LogInformation("Creating order using Customer ID: {CustomerId}", request.Request.CustomerId);
            var order = new OrderEntity()
            {
                CustomerId = customer.Id,
                ShopperAssistantId = shopperAssistant.Id,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.New,
                PickUpDeadline = DateTime.UtcNow.AddMinutes(30),
                OrderItems = new List<OrderItemsEntity>()
            };
            
            foreach (var item in request.Request.Items)
            {
                var product = await _productApi.GetProductById(item.ProductId);
                
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found while creating order", item.ProductId);
                    throw new Exceptions.ProductNotFoundException(item.ProductId);
                }
                
                if (item.Quantity <= 0)
                {
                    _logger.LogWarning("Invalid quantity for product {ProductId}", item.ProductId);
                    throw new InvalidQuantityException(item.ProductId);
                }
            
                if (item.Quantity > product.Stock)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Stock}, Requested: {Quantity}",
                        item.ProductId, product.Stock, item.Quantity);
                    throw new InsufficientStockException(item.ProductId, product.Stock, item.Quantity);
                }

                var orderItems = new OrderItemsEntity()
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };
                order.OrderItems.Add(orderItems);
                _logger.LogInformation("Adding product {ProductId} with quantity {Quantity} to order",
                    orderItems.ProductId, orderItems.Quantity);
            }
            _logger.LogInformation("Order contains {ItemCount} items", order.OrderItems.Count);

            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);

            var response = new OrderResponse
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
            return response;
        }
        catch (ApiException e)
        {
            _logger.LogError(e, 
                "External service failed. Status code: {StatusCode}",
                e.StatusCode);
            throw;
        }
    }
}