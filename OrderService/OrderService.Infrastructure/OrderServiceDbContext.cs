using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure;

public class OrderServiceDbContext : DbContext
{
    public OrderServiceDbContext(DbContextOptions<OrderServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore(typeof(DbSet<BaseEntity<long>>));
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemsEntity> OrderItems { get; set; }
}