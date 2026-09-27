namespace OrderService.Domain.Entities;

public class CartItemsEntity
{
    public long Id { get; set; }
    public long CartId { get; set; }
    public long MarketId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalItemPrice { get; set; }
}