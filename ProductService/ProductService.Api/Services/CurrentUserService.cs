using System.Security.Claims;
using ProductService.Application.Interfaces.Services;

namespace ProductService.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long UserId =>
        long.Parse(
            _httpContextAccessor.HttpContext!
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier)!
        );

    public long? MarketId
    {
        get
        {
            var marketId = _httpContextAccessor.HttpContext!
                .User
                .FindFirstValue("MarketId");

            return marketId == null ? null : long.Parse(marketId);
        }
    }

    public string Role =>
        _httpContextAccessor.HttpContext!
            .User
            .FindFirstValue(ClaimTypes.Role)!;

    public string? AdminPosition =>
        _httpContextAccessor.HttpContext!
            .User
            .FindFirstValue("AdminPosition");
}