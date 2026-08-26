namespace MarketService_Application.DTOs.Request;

public class CreateMarketRequest
{
    public long MarketAdminId { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}