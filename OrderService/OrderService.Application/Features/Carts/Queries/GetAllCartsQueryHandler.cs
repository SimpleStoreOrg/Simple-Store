using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Carts.Queries;

public record GetAllCartsQuery(int? PageNumber = null, int? PageSize = null) : IRequest<PagedResponse<CartResponse>>;

public class GetAllCartsQueryHandler : IRequestHandler<GetAllCartsQuery, PagedResponse<CartResponse>>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<GetAllCartsQueryHandler> _logger;

    public GetAllCartsQueryHandler(IOrderServiceDbContext dbContext, ILogger<GetAllCartsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<PagedResponse<CartResponse>> Handle(GetAllCartsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Cart. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
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
        
        var query = _dbContext.Carts.AsQueryable();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query =  query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var carts = await query
            .OrderBy(c => c.Id)
            .Select(c => new CartResponse
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                Items = c.CartItems.Select(ci => new CartItemResponse
                {
                    ProductId = ci.ProductId,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.Quantity * ci.Price
                }).ToList(),
                PickUpDeadline = c.PickUpDeadline,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt
            }).ToListAsync(cancellationToken);

        return new PagedResponse<CartResponse>
        {
            Items = carts,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}