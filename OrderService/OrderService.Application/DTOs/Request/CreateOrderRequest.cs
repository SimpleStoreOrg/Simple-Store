namespace OrderService.Application.DTOs.Request;

public class CreateOrderRequest
{ 
    public List<OrderItemRequest> Items { get; set; }
}