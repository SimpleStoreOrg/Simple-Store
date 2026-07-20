using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.ShopperAssistant.Queries.GetShopperAssistantById;

public class GetShopperAssistantByIdQueryHandler : IRequestHandler<GetShopperAssistantByIdQuery, UserResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetShopperAssistantByIdQueryHandler> _logger;

    public GetShopperAssistantByIdQueryHandler(IUserServiceDbContext dbContext, ILogger<GetShopperAssistantByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<UserResponse> Handle(GetShopperAssistantByIdQuery request, CancellationToken cancellationToken)
    {
        var shopperAssistant =
            await _dbContext.ShopperAssistants.FirstOrDefaultAsync(e => e.Id == request.ShopperAssistantId,
                cancellationToken);

        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {Id} not found", request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.ShopperAssistantId);
        }

        return new UserResponse()
        {
            Id = shopperAssistant.Id,
            Name = shopperAssistant.Name,
            Surname = shopperAssistant.Surname,
            Role = shopperAssistant.Role,
            Email = shopperAssistant.Email,
            PhoneNumber = shopperAssistant.PhoneNumber,
            CreatedAt = shopperAssistant.CreatedAt,
            UpdatedAt = shopperAssistant.UpdatedAt
        };
    }
}