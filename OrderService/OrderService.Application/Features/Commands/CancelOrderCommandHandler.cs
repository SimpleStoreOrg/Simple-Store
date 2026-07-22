using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record CancelOrderCommand(long OrderId) : IRequest;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(IOrderServiceDbContext dbContext, ILogger<CancelOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync(request.OrderId, cancellationToken);
        
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
        
        if (order.Status != OrderStatus.New)
        {
            throw new InvalidOrderException("Only new orders can be cancelled");
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