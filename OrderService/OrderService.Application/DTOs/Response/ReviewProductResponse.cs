namespace OrderService.Application.DTOs.Response;

public class ReviewProductResponse
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}