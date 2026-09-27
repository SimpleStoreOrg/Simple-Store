using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.External;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record CancelOrderCommand(long OrderId) : IRequest;

public class CancelOrderByCustomerCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CancelOrderByCustomerCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly IProductApi _productApi;

    public CancelOrderByCustomerCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CancelOrderByCustomerCommandHandler> logger,
        IHttpContextAccessor accessor,
        IProductApi productApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _productApi = productApi;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer ID not found.");
        }

        long customerId = long.Parse(customerIdStr);

        var order = await _dbContext.Orders.Include(o=>o.OrderItems).FirstOrDefaultAsync(
            o => o.Id == request.OrderId && o.CustomerId == customerId,
            cancellationToken);
        
        if (order == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        var today = DateTime.UtcNow.Date;
        
        var completedOrderCount = await _dbContext.Orders.CountAsync(
            o => o.CustomerId == customerId && 
                 o.Status == OrderStatus.Completed && 
                 o.CreatedAt >= today,
            cancellationToken);

        var canCancelAnyStatus = completedOrderCount >= 5;
        
        if (!canCancelAnyStatus)
        {
            if (order.Status == OrderStatus.Completed)
            {
                throw new OrderAlreadyPaidException(request.OrderId);
            }
            
            if (order.Status == OrderStatus.CancelledByShop || order.Status == OrderStatus.CancelledByCustomer)
            {
                throw new InvalidOrderException("Order is cancelled by Shop or Customer");
            }

            if (order.Status != OrderStatus.New)
            {
                throw new InvalidOrderException($"Only new orders can be cancelled. Current status: {order.Status}");
            }
        }

        try
        {
            _logger.LogInformation("Cancelling Order {OrderId}", request.OrderId);
            
            var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();

            foreach (var item in order.OrderItems)
            {
                _logger.LogInformation("Restoring stock for Product {ProductId}. Quantity: {Quantity}", item.ProductId,
                    item.Quantity);

                await _productApi.UpdateStock(item.ProductId, new UpdateStockRequest
                {
                    Quantity = -item.Quantity
                }, token);
            }
            
            order.Status = OrderStatus.CancelledByCustomer;
            
            await _dbContext.SaveChangesAsync(cancellationToken);

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occured while cancelling order for Order ID: {OrderId}",
                request.OrderId);
            throw;
        }
    }
}