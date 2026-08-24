namespace MarketService.Domain.Interfaces;

public interface IHasDeleted
{
    DateTime? DeletedAt { get; set; }
}