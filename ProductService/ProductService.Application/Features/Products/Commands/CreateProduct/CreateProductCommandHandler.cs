using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IProductServiceDbContext dbContext, ILogger<CreateProductCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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
        
        _logger.LogInformation("New Product creation");
        var product = new ProductEntity()
        {
            Name = request.Request.Name,
            Price = request.Request.Price,
            Stock = request.Request.Stock,
            CategoryId = request.Request.CategoryId
        };
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId}: {ProductName} created successfully", product.Id, product.Name);
        
        return new ProductResponse()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CreatedAt = DateTime.UtcNow
        };
    }
}