namespace OrderService.Application.DTOs.Request;

public class CreateOrderRequest
{
    public long CustomerId { get; set; }
    public long ShopperAssistantId { get; set; }
    public List<OrderItemRequest> Items { get; set; }
}