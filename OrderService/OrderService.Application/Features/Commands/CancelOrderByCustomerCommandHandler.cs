using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record CancelOrderCommand(long OrderId) : IRequest;

public class CancelOrderByCustomerCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CancelOrderByCustomerCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public CancelOrderByCustomerCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<CancelOrderByCustomerCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer ID not found.");
        }

        long customerId = long.Parse(customerIdStr);

        var order = await _dbContext.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId && o.CustomerId == customerId,
            cancellationToken);
        
        if (order == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new OrderAlreadyPaidException(request.OrderId);
        }

        if (order.Status == OrderStatus.CancelledByShop || order.Status == OrderStatus.CancelledByCustomer)
        {
            throw new InvalidOrderException("Order is cancelled by Shop or Customer");
        }

        try
        {
            _logger.LogInformation("Cancelling Order {OrderId}", request.OrderId);
            
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