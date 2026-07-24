using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Response;

public class ShopperAssistantResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public RoleStatus Role { get; set; }
    public ShopperAssistantPosition Position { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}