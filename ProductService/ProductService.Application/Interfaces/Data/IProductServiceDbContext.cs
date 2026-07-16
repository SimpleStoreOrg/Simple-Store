using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces.Data;

public interface IProductServiceDbContext
{
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    Task<int> SaveChangesAsync(CancellationToken token);
}