namespace OrderService.Application.DTOs.Response;

public class OrderItemResponse
{
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}