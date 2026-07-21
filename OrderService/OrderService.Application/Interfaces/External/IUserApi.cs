using OrderService.Application.DTOs.External;
using Refit;

namespace OrderService.Application.Interfaces.External;

public interface IUserApi
{
    [Get("/api/users/{id}")]
    Task<UserResponse> GetUserById(long id);
}