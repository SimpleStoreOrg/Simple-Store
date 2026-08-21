namespace OrderService.Application.DTOs.Request;

public class ReviewProductRequest
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Rating { get; set; }
    public string? Message { get; set; }
}