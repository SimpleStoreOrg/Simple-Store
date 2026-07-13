namespace OrderService.Domain.Entities;

public class OrderItemsEntity
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}