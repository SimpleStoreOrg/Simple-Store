using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface ICustomerApi
{
    [Get("/api/Customer/{id}")]
    Task<UserResponse?> GetCustomerById(long id, [Header("Authorization")] string? authorization);
}