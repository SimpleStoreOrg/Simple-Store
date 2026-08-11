using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Request;

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public RoleStatus Role { get; set; }
}