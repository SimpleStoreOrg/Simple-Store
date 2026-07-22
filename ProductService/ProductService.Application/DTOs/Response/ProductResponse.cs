namespace ProductService.Application.DTOs.Response;

public class ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
    public long CategoryId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}