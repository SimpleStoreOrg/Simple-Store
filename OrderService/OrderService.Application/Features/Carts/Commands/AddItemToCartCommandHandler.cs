using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Carts.Commands;

public record AddItemToCartCommand(CreateCartRequest Request) : IRequest<CartResponse>;

public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, CartResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<AddItemToCartCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly IProductApi _productApi;

    public AddItemToCartCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<AddItemToCartCommandHandler> logger,
        IHttpContextAccessor accessor,
        IProductApi productApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _productApi = productApi;
    }
    public async Task<CartResponse> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not authorized");
        }
        
        long customerId = long.Parse(customerIdStr);
        
        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        request.Request.CartItems = request.Request.CartItems
            .GroupBy(e => e.ProductId)
            .Select(g => new CartItemRequest
            {
                ProductId = g.Key,
                Quantity = g.Sum(e => e.Quantity)
            })
            .ToList();

        var productIds = request.Request.CartItems.Select(x => x.ProductId).ToArray();
        
        var productResponse = await _productApi.GetAllProducts(productIds, token);
        
        var products = productResponse.Items;

        if (products.Count == 0)
        {
            throw new Exception("No products found");
        }

        var marketId = products[0].MarketId;
        
        foreach (var product in products)
        {
            if (product.MarketId != marketId)
            {
                throw new InvalidMarketException("All products must be from the same market");
            }
        }
        
        var cart = await _dbContext.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(
                c => c.CustomerId == customerId,
                cancellationToken);

        if (cart == null)
        {
            throw new CartNotFoundException("Cart not found");
        }
        
        if (cart.CartItems.Any())
        {
            var existingMarketId = cart.CartItems.First().MarketId;

            foreach (var product in products)
            {
                if (product.MarketId != existingMarketId)
                {
                    throw new InvalidMarketException("All products in the cart must be from the same market");
                }
            }
        }

        cart.CustomerId = customerId;
        
        foreach (var item in request.Request.CartItems)
        {
            var product = products.FirstOrDefault(
                p => p.Id == item.ProductId);

            if (product == null)
            {
                throw new ProductNotFoundException(item.ProductId);
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == item.ProductId);

            decimal newQuantity;
            
            if (existingItem != null)
            {
                newQuantity = existingItem.Quantity + item.Quantity;
            }
            else
            {
                newQuantity = item.Quantity;
            }

            if (newQuantity > product.Stock)
            {
                _logger.LogWarning(
                    "Insufficient stock for product {ProductId}. " +
                    "Available: {Stock}, Requested: {Quantity}",
                    item.ProductId,
                    product.Stock,
                    newQuantity);

                throw new InsufficientStockException(
                    item.ProductId,
                    product.Stock,
                    newQuantity);
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
                existingItem.TotalItemPrice = existingItem.Quantity * existingItem.Price;
            }
            else
            {
                var newCartItem = new CartItemsEntity
                {
                    CartId = cart.Id,
                    MarketId = marketId,
                    ProductId = item.ProductId,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    TotalItemPrice = item.Quantity * product.Price
                };

                cart.CartItems.Add(newCartItem);
            }
        }

        cart.TotalPrice = cart.CartItems.Sum(item => item.TotalItemPrice);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cart updated successfully with ID: {CartId}", cart.Id);

        return new CartResponse
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Items = cart.CartItems
                .Select(ci => new CartItemResponse
                {
                    MarketId = ci.MarketId,
                    ProductId = ci.ProductId,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    TotalItemPrice = ci.TotalItemPrice
                }).ToList(),
            TotalPrice = cart.TotalPrice,
            CreatedAt = cart.CreatedAt
        };
    }
}