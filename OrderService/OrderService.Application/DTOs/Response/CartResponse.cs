namespace OrderService.Application.DTOs.Response;

public class CartResponse
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public IEnumerable<CartItemResponse> Items { get; set; }
    public DateTime? PickUpDeadline { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}