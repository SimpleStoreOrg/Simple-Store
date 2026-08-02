using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces.Data;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure;

public class ProductServiceDbContext : DbContext, IProductServiceDbContext
{
    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore(typeof(DbSet<BaseEntity<long>>));

        modelBuilder.Entity<CategoryEntity>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<ProductEntity>()
            .HasQueryFilter(x => x.DeletedAt == null);
        
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
}