using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderService.Application.Configuration;
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
    private readonly IOptions<CustomerCancellationOptions> _options;

    public CancelOrderByCustomerCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CancelOrderByCustomerCommandHandler> logger,
        IHttpContextAccessor accessor,
        IProductApi productApi,
        IOptions<CustomerCancellationOptions> options)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _productApi = productApi;
        _options = options;
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

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOrderException("Order is already completed");
        }
        
        if (order.Status == OrderStatus.CancelledByShop || order.Status == OrderStatus.CancelledByCustomer)
        {
            throw new InvalidOrderException("Order is cancelled by Shop or Customer");
        }

        if (order.Status != OrderStatus.New)
        {
            var period = _options.Value.CancellationCountPeriod;

            DateTime periodStart;

            if (period.DurationType == 1)
            {
                periodStart = DateTime.UtcNow.Date.AddDays(-(period.Duration - 1));
            }
            else if (period.DurationType == 2)
            {
                periodStart = DateTime.UtcNow.Date
                    .AddDays(-(int)DateTime.UtcNow.DayOfWeek)
                    .AddDays(-7 * (period.Duration - 1));
            }
            else if (period.DurationType == 3)
            {
                periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                    .AddMonths(-(period.Duration - 1));
            }
            else
            {
                throw new InvalidOrderException("Invalid customer cancellation period.");
            }

            var completedOrderCount = await _dbContext.Orders.CountAsync(
                o => o.CustomerId == customerId &&
                     o.Status == OrderStatus.Completed &&
                     o.CompletedAt != null &&
                     o.CompletedAt >= periodStart,
                cancellationToken);

            if (completedOrderCount < period.CompletedOrderCount)
            {
                throw new InvalidOrderException($"Customer needs at least {period.CompletedOrderCount} completed orders");
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