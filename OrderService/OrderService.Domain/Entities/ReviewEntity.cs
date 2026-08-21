using OrderService.Domain.Interfaces;

namespace OrderService.Domain.Entities;

public class ReviewEntity : IHasCreated
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}