namespace OrderService.Domain.Entities;

public class CartEntity : BaseEntity<long>
{
    public long CustomerId { get; set; }
    public DateTime PickUpDeadline { get; set; }
    public List<CartItemsEntity> CartItems { get; set; }
}