namespace UserService.Domain.Entities;

public class UserEntity : BaseEntity<long>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
    public string PasswordHash { get; set; }
}