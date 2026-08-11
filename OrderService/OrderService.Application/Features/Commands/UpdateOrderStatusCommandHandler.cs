using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record ChangeOrderStatusCommand(long OrderId, UpdateOrderStatusRequest Request) : IRequest;

public class UpdateOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(IOrderServiceDbContext dbContext,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Order Status changes");
        var order = await _dbContext.Orders.FindAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", order!.Id);
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status == OrderStatus.New || order.Status == OrderStatus.Accepted)
        {
            _logger.LogWarning("Order with ID {OrderId} must be assigned first", order.Id);
            throw new InvalidOperationException("Order must be assigned to the Shopper Assistant");
        }

        if (order.Status == OrderStatus.Completed)
        {
            _logger.LogInformation("Order with ID {OrderId} is already paid/completed", order.Id);
            throw new OrderAlreadyPaidException(request.OrderId);
        }

        if (order.Status == OrderStatus.CancelledByShop || order.Status == OrderStatus.CancelledByCustomer)
        {
            _logger.LogInformation("Order with ID {OrderId} is cancelled", order.Id);
            throw new InvalidOrderException("Order is cancelled by Shop or Customer");
        }

        try
        {
            order.Status = request.Request.Status;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}