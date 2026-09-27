using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Carts.Commands;
public record CreateCartCommand : IRequest<CartResponse>;

public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CartResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CreateCartCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public CreateCartCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CreateCartCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<CartResponse> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer is not authorized");
        }
        
        long customerId = long.Parse(customerIdStr);

        var exists = await _dbContext.Carts.AnyAsync(c => c.CustomerId == customerId, cancellationToken);

        if (exists)
        {
            throw new InvalidCartException("Cart already exists");
        }
        
        try
        {
            _logger.LogInformation("Creating cart using Customer ID: {CustomerId}", customerId);
            var cart = new CartEntity
            {
                CustomerId = customerId,
                CartItems = new List<CartItemsEntity>()
            };

            await _dbContext.Carts.AddAsync(cart, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Cart created successfully with ID: {CartId}", cart.Id);

            return new CartResponse
            {
                Id = cart.Id,
                CustomerId = cart.CustomerId,
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