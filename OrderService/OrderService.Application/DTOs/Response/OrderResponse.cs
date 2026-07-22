using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Response;

public class OrderResponse
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long ShopperAssistantId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public IEnumerable<OrderItemResponse> Items { get; set; }
    public DateTime? PickUpDeadline { get; set; }
}