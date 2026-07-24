using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class UserEntity : BaseEntity<long>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public RoleStatus Role { get; set; } = RoleStatus.Customer;
    public string PasswordHash { get; set; }
}