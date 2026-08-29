using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.External;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record PayOrderCommand(long OrderId, decimal AmountPaid) : IRequest<PaymentResponse>;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, PaymentResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<PayOrderCommandHandler> _logger;

    public PayOrderCommandHandler(IOrderServiceDbContext dbContext, ILogger<PayOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<PaymentResponse> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for Order {OrderId}", request.OrderId);

        var order = await _dbContext.Orders
            .Include(oi=>oi.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        
        if (order == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status == OrderStatus.Completed) 
        {
            throw new OrderAlreadyPaidException(request.OrderId);
        }

        if (order.Status == OrderStatus.New || order.Status == OrderStatus.Accepted ||
            order.Status == OrderStatus.Collecting)
        {
            throw new InvalidOrderException("Order must be paid when its status is Ready To Go");
        }

        if (order.Status == OrderStatus.CancelledByShop || order.Status == OrderStatus.CancelledByCustomer)
        {
            throw new InvalidOrderException("Order is cancelled by Shop or Customer");
        }

        var total = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);

        if (request.AmountPaid < total)
        {
            throw new InsufficientPaymentException(request.AmountPaid, total);
        }
        
        var change = request.AmountPaid - total;
        
        order.Status = OrderStatus.Completed;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successful payment for Order {OrderId}", request.OrderId);

        return new PaymentResponse
        {
            Total = total,
            Paid = request.AmountPaid,
            Change = change
        };
    }
}