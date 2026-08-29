using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces.Data;

public interface IOrderServiceDbContext
{
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemsEntity> OrderItems { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<CartEntity> Carts { get; set; }
    public DbSet<CartItemsEntity> CartItems { get; set; }
    Task<int> SaveChangesAsync(CancellationToken token);
}