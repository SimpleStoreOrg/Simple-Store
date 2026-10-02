namespace OrderService.Application.DTOs.Response;

public class ReviewsByProductSummaryResponse
{
    public long ProductId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewProductResponse> Reviews { get; set; } = new();
}