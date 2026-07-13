namespace UserService.Domain.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public string Token { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}