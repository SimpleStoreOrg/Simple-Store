using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface IProductApi
{
    [Get("/api/products/{id}")]
    Task<ProductResponse> GetProductById(long id);
}