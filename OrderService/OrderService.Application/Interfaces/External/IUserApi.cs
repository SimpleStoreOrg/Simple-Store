using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface IUserApi
{
    [Get("/api/User/{id}")]
    Task<UserResponse> GetUserById(long id);
}