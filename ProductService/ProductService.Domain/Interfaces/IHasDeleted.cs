namespace ProductService.Domain.Interfaces;

public interface IHasDeleted
{
    DateTime? DeletedAt { get; set; }
}