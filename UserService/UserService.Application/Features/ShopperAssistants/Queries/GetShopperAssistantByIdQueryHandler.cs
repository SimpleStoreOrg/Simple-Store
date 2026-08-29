using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.ShopperAssistants.Queries;

public record GetShopperAssistantByIdQuery(long ShopperAssistantId) : IRequest<ShopperAssistantResponse>;

public class GetShopperAssistantByIdQueryHandler : IRequestHandler<GetShopperAssistantByIdQuery, ShopperAssistantResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetShopperAssistantByIdQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetShopperAssistantByIdQueryHandler(
        IUserServiceDbContext dbContext,
        ILogger<GetShopperAssistantByIdQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<ShopperAssistantResponse> Handle(GetShopperAssistantByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        var query = _dbContext.ShopperAssistants.AsNoTracking()
            .Where(s => s.Id == request.ShopperAssistantId);
        
        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdStr);

            query = query.Where(o => o.MarketId == marketId);
        }

        var shopperAssistant = await query.FirstOrDefaultAsync(cancellationToken);

        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {Id} not found", request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.ShopperAssistantId);
        }

        return new ShopperAssistantResponse
        {
            Id = shopperAssistant.Id,
            MarketId = shopperAssistant.MarketId,
            Name = shopperAssistant.Name,
            Surname = shopperAssistant.Surname,
            Role = shopperAssistant.Role,
            Username = shopperAssistant.UserName,
            Email = shopperAssistant.Email,
            PhoneNumber = shopperAssistant.PhoneNumber,
            CreatedAt = shopperAssistant.CreatedAt,
            UpdatedAt = shopperAssistant.UpdatedAt,
            DeletedAt = shopperAssistant.DeletedAt
        };
    }
}