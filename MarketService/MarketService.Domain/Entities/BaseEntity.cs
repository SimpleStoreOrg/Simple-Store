using MarketService.Domain.Interfaces;

namespace MarketService.Domain.Entities;

public class BaseEntity<T> : IHasCreated, IHasDeleted, IHasUpdated
{
    public T Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}