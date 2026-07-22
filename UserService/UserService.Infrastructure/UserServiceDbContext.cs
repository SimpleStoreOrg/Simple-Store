using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Entities;

namespace UserService.Infrastructure;

public class UserServiceDbContext : DbContext, IUserServiceDbContext
{
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore(typeof(DbSet<BaseEntity<long>>));
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<ShopperAssistantEntity> ShopperAssistants { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}