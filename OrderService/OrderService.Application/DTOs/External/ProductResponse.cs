namespace OrderService.Application.DTOs.External;

public class ProductResponse
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
    public long CategoryId { get; set; }
}