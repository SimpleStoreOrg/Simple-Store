using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure;

public class ProductServiceDbContext : DbContext
{
    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore(typeof(DbSet<BaseEntity<long>>));
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
}