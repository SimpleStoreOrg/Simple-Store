using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.Products.Commands;

public record UpdateProductCommand(int ProductId, UpdateProductRequest Request) : IRequest<ProductResponse>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public UpdateProductCommandHandler(
        IProductServiceDbContext dbContext,
        ILogger<UpdateProductCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", request.ProductId);
        
        var name = request.Request.Name.Trim().ToLower();

        var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;
        
        if (marketIdStr == null)
        {
            throw new NotAuthorizedException("Market ID not found/authorized");
        }

        long marketId = long.Parse(marketIdStr);
        
        var exists = await _dbContext.Products
            .AnyAsync(p =>
                p.Id != request.ProductId &&
                p.Name.ToLower() == name && p.MarketId == marketId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Product already exists with name: {ProductName}", name);
            throw new ProductAlreadyExistsException();
        }
        
        var product = await _dbContext.Products.FirstOrDefaultAsync(p=>p.Id == request.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with {ProductId} not found", request.ProductId);
            throw new ProductNotFoundException(request.ProductId);
        }

        product.Name = request.Request.Name;
        product.Price = request.Request.Price;
        product.Stock = request.Request.Stock;
        product.CategoryId = request.Request.CategoryId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Product {ProductId} updated successfully. Name: {Name}, Price: {Price}",
            product.Id, product.Name, product.Price);
        
        return new ProductResponse
        {
            Id = product.Id,
            MarketId = product.MarketId,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}