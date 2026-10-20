using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record UpdateOrderStatusCommand(long OrderId, UpdateOrderStatusRequest Request) : IRequest;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
    private readonly ICustomerApi _customerApi;
    private readonly IHttpContextAccessor _accessor;
    private readonly IEmailService _emailService;

    public UpdateOrderStatusCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<UpdateOrderStatusCommandHandler> logger,
        IEmailService emailService,
        ICustomerApi customerApi,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _customerApi = customerApi;
        _emailService = emailService;
        _accessor = accessor;
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

        if (request.Request.Status == OrderStatus.New || 
            request.Request.Status == OrderStatus.Completed ||
            request.Request.Status == OrderStatus.CancelledByCustomer || 
            request.Request.Status == OrderStatus.Accepted)
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
            var previousStatus = order.Status;

            order.Status = request.Request.Status;

            if (previousStatus != OrderStatus.Completed && request.Request.Status == OrderStatus.Completed)
            {
                _logger.LogInformation("Order {OrderId} completed at: {CompletedAt}", order.Id, order.CompletedAt);
                order.CompletedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            await TryNotifyCustomerAsync(order, cancellationToken);
        }
        
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task TryNotifyCustomerAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        var notifyFor = new[]
        {
            OrderStatus.Collecting, OrderStatus.ReadyToGo, OrderStatus.Completed
        };

        if (!notifyFor.Contains(order.Status))
        {
            return;
        }

        try
        {
            var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
            var customer = await _customerApi.GetCustomerById(order.CustomerId, token);
            if (customer == null)
            {
                _logger.LogWarning("No email for customer {CustomerId}", order.CustomerId);
                return;
            }

            await _emailService.SendStatusEmailAsync(
                customer.Email, order.Id, order.Status.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order {OrderId} status changed to {Status} but email notification failed",
                order.Id, order.Status);
        }
    }
}