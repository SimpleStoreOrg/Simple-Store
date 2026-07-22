using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.Commands;

public record UpdateStockCommand(long ProductId, decimal Quantity) : IRequest;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand>
{
    private readonly IProductServiceDbContext _dbContext;

    public UpdateStockCommandHandler(IProductServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        if (product.Stock < request.Quantity)
        {
            throw new Exception("Not enough stock");
        }

        product.Stock -= request.Quantity;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}