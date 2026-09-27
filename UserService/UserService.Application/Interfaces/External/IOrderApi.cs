using Refit;

namespace UserService.Application.Interfaces.External;

public interface IOrderApi
{
    [Get("/api/Order/internal/customerids")]
    Task<List<long>> GetMarketCustomerIds(
        [Header("Authorization")] string? authorization);
}