namespace ProductService.Domain.Entities;

public class ProductEntity : BaseEntity<long>
{
    public long MarketId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public decimal Stock { get; set; }
    public long CategoryId { get; set; }
}