namespace OrderService.Domain.Entities;

public class CartEntity : BaseEntity<long>
{
    public long CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<CartItemsEntity> CartItems { get; set; }
}