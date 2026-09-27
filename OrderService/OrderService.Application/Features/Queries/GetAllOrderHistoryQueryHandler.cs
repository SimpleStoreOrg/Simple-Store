using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Queries;

public record GetAllOrderHistoryQuery(
    int? PageNumber = null,
    int? PageSize = null,
    DateTime? OrderedFrom = null,
    DateTime? OrderedTo = null) : IRequest<PagedResponse<OrderResponse>>;

public class GetAllOrderHistoryQueryHandler : IRequestHandler<GetAllOrderHistoryQuery, PagedResponse<OrderResponse>>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetAllOrderHistoryQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetAllOrderHistoryQueryHandler(
        IOrderServiceDbContext dbContext,
        ILogger<GetAllOrderHistoryQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<PagedResponse<OrderResponse>> Handle(GetAllOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Order History. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
        if (request.PageNumber.HasValue && request.PageNumber.Value <= 0)
        {
            _logger.LogWarning("Page number {PageNumber}, must be greater than 0", request.PageNumber);
            throw new IncorrectPaginationException("Page number must be greater than 0.");
        }

        if (request.PageSize.HasValue && request.PageSize.Value <= 0)
        {
            _logger.LogWarning("Page size {PageSize}, must be greater than 0", request.PageSize);
            throw new IncorrectPaginationException("Page size must be greater than 0.");
        }

        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (customerIdStr == null)
        {
            throw new NotAuthorizedException("Customer not found");
        }

        long customerId = long.Parse(customerIdStr);

        var query = _dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .AsQueryable();

        if (query == null)
        {
            throw new OrderHistoryNotFoundException($"No order history found for Customer ID: {customerId}");
        }
        
        if (request.OrderedFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.OrderedFrom);
        }

        if (request.OrderedTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.OrderedTo);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }
        
        var orders = await query
            .OrderBy(o=>o.Id)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                ShopperAssistantId = o.ShopperAssistantId,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Items = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    MarketId = oi.MarketId,
                    ProductId = oi.ProductId,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    TotalItemPrice = oi.Quantity * oi.Price
                }).ToList(),
                TotalPrice = o.TotalPrice,
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