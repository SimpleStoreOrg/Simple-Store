using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Carts.Commands;

public record RemoveCartItemCommand(long ProductId) : IRequest<CartResponse>;

public class RemoveCartItemCommandHandler
    : IRequestHandler<RemoveCartItemCommand, CartResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<RemoveCartItemCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public RemoveCartItemCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<RemoveCartItemCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task<CartResponse> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (customerIdStr == null)
        {
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
        
        var cartItem = cart.CartItems
            .FirstOrDefault(item => item.ProductId == request.ProductId);

        if (cartItem == null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        cart.CartItems.Remove(cartItem);

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