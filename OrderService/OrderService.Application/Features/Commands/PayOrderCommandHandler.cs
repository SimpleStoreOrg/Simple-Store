using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Enums;
using ProductNotFoundException = ProductService.Application.Exceptions.ProductNotFoundException;

namespace OrderService.Application.Features.Commands;

public record PayOrderCommand(long OrderId, decimal AmountPaid) : IRequest<PaymentResponse>;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, PaymentResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<PayOrderCommandHandler> _logger;
    private readonly IProductApi _productApi;

    public PayOrderCommandHandler(IOrderServiceDbContext dbContext, ILogger<PayOrderCommandHandler> logger,
        IProductApi productApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _productApi = productApi;
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
        
        try
        {
            foreach (var item in order.OrderItems)
            {
                var product = await _productApi.GetProductById(item.ProductId);
                
                if (product == null)
                {
                    throw new ProductNotFoundException(item.ProductId);
                }

                if (product.Stock < item.Quantity)
                {
                    throw new InsufficientStockException(item.ProductId, product.Stock, item.Quantity);
                }
                
                product.Stock -= item.Quantity;
            }

            order.Status = OrderStatus.Completed;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successful payment");

            return new PaymentResponse()
            {
                Total = total,
                Paid = request.AmountPaid,
                Change = change
            };
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occured while paying order for Customer ID: {CustomerId}",
                order.CustomerId);
            throw;
        }
    }
}