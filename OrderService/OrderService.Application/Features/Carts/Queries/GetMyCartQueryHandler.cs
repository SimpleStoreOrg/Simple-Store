using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Carts.Queries;

public record GetMyCartQuery : IRequest<CartResponse>;

public class GetMyCartQueryHandler
    : IRequestHandler<GetMyCartQuery, CartResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetMyCartQueryHandler(
        IOrderServiceDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CartResponse> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        var customerIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (customerIdClaim == null)
        {
            throw new UnauthorizedAccessException("Customer ID was not found in the token");
        }

        long customerId = long.Parse(customerIdClaim);

        var cart = await _dbContext.Carts
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CartResponse
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                Items = c.CartItems
                    .OrderBy(ci => ci.Id)
                    .Select(ci => new CartItemResponse
                    {
                        ProductId = ci.ProductId,
                        MarketId = ci.MarketId,
                        Price = ci.Price,
                        Quantity = ci.Quantity,
                        TotalItemPrice = ci.Quantity * ci.Price
                    })
                    .ToList(),
                TotalPrice = c.TotalPrice,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null)
        {
            throw new CartNotFoundException("Cart not found");
        }

        return cart;
    }
}