namespace MarketService_Application.DTOs.Request;

public class UpdateMarketRequest
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}