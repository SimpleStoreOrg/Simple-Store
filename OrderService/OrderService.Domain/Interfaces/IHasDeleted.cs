namespace OrderService.Domain.Interfaces;

public interface IHasDeleted
{
    DateTime? DeletedAt { get; set; }
}