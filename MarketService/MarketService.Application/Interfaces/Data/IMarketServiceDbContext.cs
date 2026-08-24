using MarketService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketService_Application.Interfaces.Data;

public interface IMarketServiceDbContext 
{
    public DbSet<MarketEntity> Markets { get; set; }
}