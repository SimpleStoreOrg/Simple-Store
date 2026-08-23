namespace OrderService.Application.DTOs.External;

public class TopProductsByCategoryResponse
{
    public long CategoryId { get; set; }
    public List<TopProductsResponse> Products { get; set; } = new();
}