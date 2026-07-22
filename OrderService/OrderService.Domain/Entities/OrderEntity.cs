using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class OrderEntity : BaseEntity<long>
{
    public long CustomerId { get; set; }
    public long ShopperAssistantId { get; set; }
    public DateTime PickUpDeadline { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemsEntity> OrderItems { get; set; }
}