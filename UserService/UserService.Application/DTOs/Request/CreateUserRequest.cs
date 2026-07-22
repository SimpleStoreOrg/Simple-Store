namespace UserService.Application.DTOs.Request;

public class CreateUserRequest
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
}