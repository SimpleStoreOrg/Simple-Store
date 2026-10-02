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

namespace OrderService.Application.Features.Carts.Commands;

public record UpdateCartItemCommand(long ProductId, UpdateCartItemRequest Request) : IRequest<CartResponse>;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<UpdateCartItemCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly IProductApi _productApi;

    public UpdateCartItemCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<UpdateCartItemCommandHandler> logger,
        IHttpContextAccessor accessor,
        IProductApi productApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _productApi = productApi;
    }

    public async Task<CartResponse> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not authorized");
        }

        long customerId = long.Parse(customerIdStr);

        var cart = await _dbContext.Carts.Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (cart == null)
        {
            throw new CartNotFoundException("Cart not found");
        }

        var cartItem = cart.CartItems
            .FirstOrDefault(item => item.ProductId == request.ProductId);

        if (cartItem == null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();

        var product = await _productApi.GetProductById(request.ProductId, token);

        if (product == null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        if (request.Request.Quantity > product.Stock)
        {
            _logger.LogWarning(
                "Insufficient stock for product {ProductId}. Available: {Stock}, Requested: {Quantity}",
                request.ProductId,
                product.Stock,
                request.Request.Quantity);

            throw new InsufficientStockException(
                request.ProductId,
                product.Stock,
                request.Request.Quantity);
        }

        cartItem.Quantity = request.Request.Quantity;

        cartItem.TotalItemPrice = cartItem.Quantity * cartItem.Price;

        cart.TotalPrice = cart.CartItems.Sum(item => item.TotalItemPrice);

        await _dbContext.SaveChangesAsync(cancellationToken);

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
                })
                .ToList(),
            TotalPrice = cart.TotalPrice,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            DeletedAt = cart.DeletedAt
        };
    }
}