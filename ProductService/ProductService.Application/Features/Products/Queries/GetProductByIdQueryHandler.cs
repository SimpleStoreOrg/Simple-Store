using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.Products.Queries;

public record GetProductByIdQuery(int ProductId) : IRequest<ProductResponse>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetProductByIdQueryHandler(
        IProductServiceDbContext dbContext,
        ILogger<GetProductByIdQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<ProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        var query = _dbContext.Products.AsNoTracking().Where(p => p.Id == request.ProductId);
        
        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdStr);

            query = query.Where(p => p.MarketId == marketId);
        }

        var product = await query.FirstOrDefaultAsync(cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", request.ProductId);
            throw new ProductNotFoundException(request.ProductId);
        }
        
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            DeletedAt = product.DeletedAt
        };
    }
}