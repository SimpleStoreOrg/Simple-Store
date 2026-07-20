using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderServiceDbContext dbContext, ILogger<CreateOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    // {
    //     
    // }
}