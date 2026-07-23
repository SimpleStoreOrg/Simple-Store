using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.ShopperAssistant.Commands;

public record DeleteShopperAssistantCommand(long ShopperAssistantId) : IRequest, IRequest<bool>;

public class DeleteShopperAssistantCommandHandler : IRequestHandler<DeleteShopperAssistantCommand, bool>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<DeleteShopperAssistantCommandHandler> _logger;

    public DeleteShopperAssistantCommandHandler(IUserServiceDbContext dbContext, ILogger<DeleteShopperAssistantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<bool> Handle(DeleteShopperAssistantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Shopper Assistant with ID: {Id}", request.ShopperAssistantId);

        var shopperAssistant =
            await _dbContext.ShopperAssistants.FirstOrDefaultAsync(e => e.Id == request.ShopperAssistantId,
                cancellationToken);
        
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