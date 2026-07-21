namespace OrderService.Application.DTOs.Request;

public class OrderItemRequest
{
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
}