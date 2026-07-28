using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Request;

public class UpdateShopperAssistantRequest
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public ShopperAssistantPosition Position { get; set; }
    public string PhoneNumber { get; set; }
}