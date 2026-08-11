using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface IShopperAssistantApi
{
    [Get("/api/ShopperAssistant/{id}")]
    Task<UserResponse?> GetShopperAssistantById(long id, [Header("Authorization")] string? authorization);
}