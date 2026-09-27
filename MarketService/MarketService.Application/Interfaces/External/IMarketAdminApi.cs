using MarketService_Application.DTOs.External;
using Refit;

namespace MarketService_Application.Interfaces.External;

public interface IMarketAdminApi
{
    [Get("/api/Admin/{id}")]
    Task<UserResponse?> GetMarketAdminById(long id, [Header("Authorization")] string? authorization);

    [Put("/api/Admin/internal/{id}/assignmarket")]
    Task<UserResponse?> AssignMarketToAdmin(long id, [Body] AssignMarketRequest request,
        [Header("Authorization")] string? authorization);
}