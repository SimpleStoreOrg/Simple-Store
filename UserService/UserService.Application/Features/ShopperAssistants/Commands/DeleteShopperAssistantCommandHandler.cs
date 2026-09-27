using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.ShopperAssistants.Commands;

public record DeleteShopperAssistantCommand(long ShopperAssistantId) : IRequest<bool>;

public class DeleteShopperAssistantCommandHandler : IRequestHandler<DeleteShopperAssistantCommand, bool>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<DeleteShopperAssistantCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public DeleteShopperAssistantCommandHandler(
        IUserServiceDbContext dbContext,
        ILogger<DeleteShopperAssistantCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<bool> Handle(DeleteShopperAssistantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Shopper Assistant with ID: {Id}", request.ShopperAssistantId);

        var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;
        
        if (marketIdStr == null)
        {
            throw new NotAuthorizedException("Market ID not found/authorized");
        }

        long marketId = long.Parse(marketIdStr);

        var shopperAssistant =
            await _dbContext.ShopperAssistants.FirstOrDefaultAsync(
                s => s.MarketId == marketId && s.Id == request.ShopperAssistantId, cancellationToken);
        
        if (shopperAssistant == null)
        {
            _logger.LogWarning("Shopper Assistant with ID {Id} not found", request.ShopperAssistantId);
            throw new ShopperAssistantNotFoundException(request.ShopperAssistantId);
        }

        _dbContext.ShopperAssistants.Remove(shopperAssistant);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Shopper Assistant deleted successfully with ID: {EmployeeId}", request.ShopperAssistantId);

        return true;
    }
}