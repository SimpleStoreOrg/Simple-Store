using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class ShopperAssistantEntity : UserEntity
{
    public ShopperAssistantPosition Position { get; set; }
}