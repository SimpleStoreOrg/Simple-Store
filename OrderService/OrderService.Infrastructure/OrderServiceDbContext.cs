using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Infrastructure;

public class OrderServiceDbContext : DbContext, IOrderServiceDbContext
{
    public OrderServiceDbContext(DbContextOptions<OrderServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore(typeof(DbSet<BaseEntity<long>>));

        modelBuilder.Entity<OrderEntity>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<CartEntity>()
            .HasQueryFilter(x => x.DeletedAt == null);
        
        modelBuilder.Entity<OrderItemsEntity>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemsEntity> OrderItems { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<CartEntity> Carts { get; set; }
    public DbSet<CartItemsEntity> CartItems { get; set; }
}