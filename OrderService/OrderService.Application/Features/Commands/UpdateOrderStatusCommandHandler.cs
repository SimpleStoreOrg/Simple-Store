using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record UpdateOrderStatusCommand(long OrderId, UpdateOrderStatusRequest Request) : IRequest;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(IOrderServiceDbContext dbContext,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Order Status changes");
        var order = await _dbContext.Orders.FindAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", order!.Id);
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status == OrderStatus.New)
        {
            _logger.LogWarning("Order with ID {OrderId} must be assigned first", order.Id);
            throw new InvalidOrderException("Order must be assigned to the Shopper Assistant");
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

        if (request.Request.Status == OrderStatus.New || request.Request.Status == OrderStatus.Completed ||
            request.Request.Status == OrderStatus.CancelledByCustomer || request.Request.Status == OrderStatus.Accepted)
        {
            _logger.LogWarning("Requested status cannot be New, Accepted, Completed, and Cancelled by Customer");
            throw new InvalidOrderException("Wrong request");
        }

        if ((order.Status == OrderStatus.Accepted && request.Request.Status != OrderStatus.Collecting) ||
            (order.Status == OrderStatus.Collecting && request.Request.Status != OrderStatus.ReadyToGo) ||
            (order.Status == OrderStatus.ReadyToGo && request.Request.Status != OrderStatus.Completed) ||
            order.Status == request.Request.Status) 
        {
            _logger.LogWarning("Requested status must be in ascending order, not itself again also");
            throw new InvalidOrderException("Wrong request");
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