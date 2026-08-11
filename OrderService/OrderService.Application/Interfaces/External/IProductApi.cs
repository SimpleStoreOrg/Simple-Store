using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface IProductApi
{
    [Get("/api/Product/{id}")]
    Task<ProductResponse> GetProductById(long id, [Header("Authorization")] string? authorization);

    [Put("/api/Product/internal/{id}/stock")]
    Task UpdateStock(long id, UpdateStockRequest request, [Header("Authorization")] string? authorization);
}