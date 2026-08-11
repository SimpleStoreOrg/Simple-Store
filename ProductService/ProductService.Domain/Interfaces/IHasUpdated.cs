namespace ProductService.Domain.Interfaces;

public interface IHasUpdated
{
    DateTime? UpdatedAt { get; set; }
}