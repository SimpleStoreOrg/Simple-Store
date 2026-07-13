namespace ProductService.Domain.Entities;

public class ProductEntity : BaseEntity<long>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public decimal Stock { get; set; }
    public long CategoryId { get; set; }
}