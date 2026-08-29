namespace OrderService.Application.DTOs.Request;

public class CartItemRequest
{
    public long MarketId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
}