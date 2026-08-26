using MarketService_Application.DTOs.External;
using Refit;

namespace MarketService_Application.Interfaces.External;

public interface IMarketAdminApi
{
    [Get("/api/Admin/{id}")]
    Task<UserResponse?> GetMarketAdminById(long id, [Header("Authorization")] string? authorization);
}