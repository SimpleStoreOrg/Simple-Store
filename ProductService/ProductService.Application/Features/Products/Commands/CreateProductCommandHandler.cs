using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;
using ProductService.Application.Interfaces.External;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductResponse>;

public class CreateProductCommandHandler: IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly IMarketApi _marketApi;
    private readonly IHttpContextAccessor _accessor;

    public CreateProductCommandHandler(IProductServiceDbContext dbContext,
        ILogger<CreateProductCommandHandler> logger,
        IMarketApi marketApi,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _marketApi = marketApi;
        _accessor = accessor;
    }
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var name = request.Request.Name.Trim().ToLower();
        
        var exists = await _dbContext.Products.AnyAsync(p => p.Name.Trim().ToLower() == name, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Product already exists with name: {ProductName}", name);
            throw new ProductAlreadyExistsException();
        }

        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();

        var market = await _marketApi.GetMarketById(request.Request.MarketId, token);

        if (market == null)
        {
            _logger.LogWarning("Market with ID {MarketId} not found", request.Request.MarketId);
            throw new MarketNotFoundException(request.Request.MarketId);
        }
        
        _logger.LogInformation("New Product creation");
        var product = new ProductEntity
        {
            MarketId = market.Id,
            Name = request.Request.Name,
            Price = request.Request.Price,
            Stock = request.Request.Stock,
            CategoryId = request.Request.CategoryId
        };
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId}: {ProductName} created successfully", product.Id, product.Name);
        
        return new ProductResponse
        {
            Id = product.Id,
            MarketId = product.MarketId,
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