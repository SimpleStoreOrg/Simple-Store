using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure;

public class OrderServiceDbContext : DbContext, IOrderServiceDbContext
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

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}