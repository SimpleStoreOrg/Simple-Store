using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class AdminEntity : UserEntity
{
    public AdminPosition Position { get; set; }
}