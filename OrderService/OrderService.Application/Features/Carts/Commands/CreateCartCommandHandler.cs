using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Carts.Commands;
public record CreateCartCommand(CreateCartRequest Request) : IRequest<CartResponse>;

public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CartResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CreateCartCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly IProductApi _productApi;

    public CreateCartCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CreateCartCommandHandler> logger,
        IHttpContextAccessor accessor,
        IProductApi productApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _productApi = productApi;
    }
    public async Task<CartResponse> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not authorized");
        }
        
        long customerId = long.Parse(customerIdStr);
        
        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        if (request.Request.CartItems == null || request.Request.CartItems.Count <= 0)
        {
            throw new InvalidCartException("Order must contain at least one item");
        }

        request.Request.CartItems = request.Request.CartItems
            .DistinctBy(e => e.ProductId)
            .ToList();
        
        try
        {
            _logger.LogInformation("Creating cart using Customer ID: {CustomerId}", customerId);
            var cart = new CartEntity
            {
                CustomerId = customerId,
                PickUpDeadline = DateTime.UtcNow.AddHours(5).AddMinutes(30), 
                CartItems = new List<CartItemsEntity>()
            };

            foreach (var item in request.Request.CartItems)
            {
                if (item.Quantity <= 0)
                {
                    _logger.LogWarning("Invalid quantity for product {ProductId}", item.ProductId);
                    throw new InvalidQuantityException(item.ProductId);
                }

                var product = await _productApi.GetProductById(item.ProductId, token);

                if (item.MarketId != product.MarketId)
                {
                    throw new InvalidMarketException("Products of the cart must be from the same market");
                }

                if (item.Quantity > product.Stock)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Stock}, Requested: {Quantity}",
                        item.ProductId, product.Stock, item.Quantity);
                    throw new InsufficientStockException(item.ProductId, product.Stock, item.Quantity);
                }

                var items = new CartItemsEntity
                {
                    CartId = cart.Id,
                    MarketId = product.MarketId,
                    ProductId = item.ProductId,
                    Price = product.Price,
                    Quantity = item.Quantity
                };
                
                cart.CartItems.Add(items);
                _logger.LogInformation("Adding product {ProductId} with quantity {Quantity} to cart",
                    items.ProductId, items.Quantity);
            }
            
            _logger.LogInformation("Cart contains {ItemCount} items", cart.CartItems.Count);

            await _dbContext.Carts.AddAsync(cart, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Cart created successfully with ID: {CartId}", cart.Id);

            return new CartResponse
            {
                Id = cart.Id,
                CustomerId = cart.CustomerId,
                Items = cart.CartItems.Select(ci => new CartItemResponse
                {
                    MarketId = ci.MarketId,
                    ProductId = ci.ProductId,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.Quantity * ci.Price
                }).ToList(),
                PickUpDeadline = cart.PickUpDeadline,
                CreatedAt = cart.CreatedAt
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}