using MarketService_Application.Interfaces.Data;
using MarketService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketService.Infrastructure;

public class MarketServiceDbContext : DbContext, IMarketServiceDbContext
{
    public MarketServiceDbContext(DbContextOptions<MarketServiceDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore(typeof(DbSet<BaseEntity<long>>));

        modelBuilder.Entity<MarketEntity>()
            .HasQueryFilter(x => x.DeletedAt == null);
        
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<MarketEntity> Markets { get; set; }
}