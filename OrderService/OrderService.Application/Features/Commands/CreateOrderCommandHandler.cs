using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

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
        var user = await _userApi.GetUserById(request.Request.CustomerId);
        
        if (user == null)
        {
            throw new Exception($"Customer not found");
        }

        try
        {
            var order = new OrderEntity()
            {
                CustomerId = user.Id,
                ShopperAssistantId = request.Request.ShopperAssistantId,
                CreatedAt = DateTime.UtcNow,
                PickUpDeadline = DateTime.UtcNow.AddMinutes(30),
                OrderItems = new List<OrderItemsEntity>()
            };
            
            foreach (var item in request.Request.Items)
            {
                var product = await _productApi.GetProductById(item.ProductId);
                
                if (product == null)
                {
                    throw new Exception("Product not found");
                }

                var orderItems = new OrderItemsEntity()
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };
                order.OrderItems.Add(orderItems);
            }

            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                ShopperAssistantId = order.ShopperAssistantId,
                Status = OrderStatus.ReadyToGo,
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}