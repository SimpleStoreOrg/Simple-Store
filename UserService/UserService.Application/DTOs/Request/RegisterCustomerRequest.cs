namespace UserService.Application.DTOs.Request;

public class RegisterCustomerRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}