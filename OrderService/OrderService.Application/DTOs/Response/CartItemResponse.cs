namespace OrderService.Application.DTOs.Response;

public class CartItemResponse
{
    public long MarketId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalItemPrice { get; set; }
}