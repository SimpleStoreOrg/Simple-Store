using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Features.Commands;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Carts.Commands;
public record CheckoutCartCommand : IRequest<OrderResponse>;

public class CheckoutCartCommandHandler : IRequestHandler<CheckoutCartCommand, OrderResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CheckoutCartCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly IMediator _mediator;
    
    public CheckoutCartCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CheckoutCartCommandHandler> logger,
        IHttpContextAccessor accessor,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _mediator = mediator;
    }
    public async Task<OrderResponse> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
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

        if (cart.CartItems.Count == 0)
        {
            throw new CartIsEmptyException();
        }
        
        var orderItems = cart.CartItems
            .Select(cartItem => new OrderItemRequest
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity
            })
            .ToList();

        var createOrderRequest = new CreateOrderRequest
        {
            Items = orderItems
        };

        var orderResponse = await _mediator.Send(new CreateOrderCommand(createOrderRequest), cancellationToken);
        
        _dbContext.Carts.Remove(cart);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return orderResponse;
    }
}