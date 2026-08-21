namespace OrderService.Application.DTOs.External;

public class TopProductsResponse
{
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal Price { get; set; }
    public decimal SoldQuantity { get; set; }
}