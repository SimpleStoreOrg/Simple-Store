using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Queries;

public record GetAllOrdersQuery(
    int PageNumber,
    int PageSize,
    long[]? CustomerIds = null,
    long[]? ShopperAssistantIds = null,
    OrderStatus? Statuses = null,
    DateTime? CreatedAtFrom = null,
    DateTime? CreatedAtTo = null) : IRequest<PagedResponse<OrderResponse>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResponse<OrderResponse>>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetAllOrdersQueryHandler> _logger;

    public GetAllOrdersQueryHandler(IOrderServiceDbContext dbContext, ILogger<GetAllOrdersQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<PagedResponse<OrderResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Order. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
        if (request.PageNumber <= 0 || request.PageSize <= 0)
        {
            _logger.LogWarning("Invalid pagination parameters. Page: {PageNumber}, Size: {PageSize}",
                request.PageNumber, request.PageSize);
            return new PagedResponse<OrderResponse>();
        }
        
        var query = _dbContext.Orders.AsQueryable();

        if (request.CustomerIds != null && request.CustomerIds.Length > 0) 
        {
            query = query.Where(o=> request.CustomerIds.Contains(o.CustomerId));
        }

        if (request.ShopperAssistantIds != null && request.ShopperAssistantIds.Length > 0)
        {
            query = query.Where(o => request.ShopperAssistantIds.Contains(o.ShopperAssistantId));
        }

        if (request.Statuses.HasValue)
        {
            query = query.Where(o => o.Status == request.Statuses.Value);
        }

        if (request.CreatedAtFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.CreatedAtFrom);
        }

        if (request.CreatedAtTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.CreatedAtTo);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query.OrderBy(o => o.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                ShopperAssistantId = o.ShopperAssistantId,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                DeletedAt = o.DeletedAt,
                Items = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.Quantity * oi.Price
                }).ToList(),
                PickUpDeadline = o.PickUpDeadline
            }).ToListAsync(cancellationToken);
        
        _logger.LogInformation("Returned {Count} order out of {Total}", orders.Count, totalCount);

        
        return new PagedResponse<OrderResponse>
        {
            Items = orders,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}