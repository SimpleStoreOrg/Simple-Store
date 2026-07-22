using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface IProductApi
{
    [Get("/api/Product/{id}")]
    Task<ProductResponse> GetProductById(long id);
}