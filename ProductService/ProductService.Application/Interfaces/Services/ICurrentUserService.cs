namespace ProductService.Application.Interfaces.Services;

public interface ICurrentUserService
{
    long UserId { get; }
    long? MarketId { get; }
    string Role { get; }
    string? AdminPosition { get; }
}