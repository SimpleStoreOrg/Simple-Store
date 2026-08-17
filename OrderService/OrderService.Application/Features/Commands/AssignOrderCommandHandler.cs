using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public record AssignOrderCommand(long OrderId, AssignOrderRequest Request) : IRequest;

public class AssignOrderCommandHandler : IRequestHandler<AssignOrderCommand>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<AssignOrderCommandHandler> _logger;
    private readonly IShopperAssistantApi _shopperAssistantApi;
    private readonly IHttpContextAccessor _accessor;

    public AssignOrderCommandHandler(
        IOrderServiceDbContext dbContext,
        ILogger<AssignOrderCommandHandler> logger,
        IShopperAssistantApi shopperAssistantApi,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _shopperAssistantApi = shopperAssistantApi;
        _accessor = accessor;
    }
    public async Task Handle(AssignOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId && o.Status == OrderStatus.New,
            cancellationToken: cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found",
                order?.Id);
            throw new OrderNotFoundException(order!.Id);
        }

        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();

        var shopperAssistant =
            await _shopperAssistantApi.GetShopperAssistantById(request.Request.ShopperAssistantId, token);
        
        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {ShopperAssistantId} not found",
                request.Request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.Request.ShopperAssistantId);
        }

        order.ShopperAssistantId = request.Request.ShopperAssistantId;
        order.Status = OrderStatus.Accepted;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order assigned to shopper assistant successfully. Order ID: {OrderId}, Shopper Assistant ID: {ShAsId}",
            order.Id, order.ShopperAssistantId);
    }
}