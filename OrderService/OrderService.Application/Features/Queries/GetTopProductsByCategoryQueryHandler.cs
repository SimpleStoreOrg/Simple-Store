using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Common;
using OrderService.Application.DTOs.External;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Application.Interfaces.External;
using IncorrectPaginationException = OrderService.Application.Exceptions.IncorrectPaginationException;

namespace OrderService.Application.Features.Queries;

public record GetTopProductsByCategoryQuery(
    int? PageNumber = null,
    int? PageSize = null,
    long[]? CategoryIds = null)
    : IRequest<PagedResponse<TopProductsByCategoryResponse>>;

public class
    GetTopProductsByCategoryQueryHandler : IRequestHandler<GetTopProductsByCategoryQuery,
    PagedResponse<TopProductsByCategoryResponse>>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly IProductApi _productApi;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<GetTopProductsByCategoryQueryHandler> _logger;

    public GetTopProductsByCategoryQueryHandler(IOrderServiceDbContext dbContext,
        IProductApi productApi,
        ILogger<GetTopProductsByCategoryQueryHandler> logger, 
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _productApi = productApi;
        _accessor = accessor;
        _logger = logger;
    }

    public async Task<PagedResponse<TopProductsByCategoryResponse>> Handle(GetTopProductsByCategoryQuery request,
        CancellationToken cancellationToken)

    {       
        _logger.LogInformation("Fetching Top Products. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
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
        
        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        var salesQuery = _dbContext.OrderItems.AsQueryable();

        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var marketIdClaim = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdClaim == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdClaim);

            salesQuery = salesQuery.Where(oi => oi.MarketId == marketId);
        }
        
        var sales = await salesQuery
            .GroupBy(oi => oi.ProductId)
            .Select(o => new TopProductsResponse
            {
                ProductId = o.Key,
                SoldQuantity = o.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.SoldQuantity)
            .ToListAsync(cancellationToken);

        var categories = new List<TopProductsByCategoryResponse>();

        foreach (var sale in sales)
        {
            var product = await _productApi.GetProductById(sale.ProductId, token);

            var category = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);

            if (category == null)
            {
                category = new TopProductsByCategoryResponse
                {
                    CategoryId = product.CategoryId
                };
                categories.Add(category);
            }

            category.Products.Add(new TopProductsResponse
            {
                ProductId = sale.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                SoldQuantity = sale.SoldQuantity
            });
        }

        if (request.CategoryIds != null && request.CategoryIds.Length > 0)
        {
            categories = categories.Where(c => request
                .CategoryIds.Contains(c.CategoryId)).ToList();
        }

        var totalCount = categories.Count;
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            categories = categories
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value).ToList();
        }
        
        return new PagedResponse<TopProductsByCategoryResponse>
        {
            Items = categories,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}