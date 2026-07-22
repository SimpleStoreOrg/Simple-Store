namespace OrderService.Application.DTOs.External;

public class ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
}