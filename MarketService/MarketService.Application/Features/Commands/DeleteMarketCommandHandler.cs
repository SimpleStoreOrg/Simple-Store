using MarketService_Application.DTOs.Response;
using MarketService_Application.Exceptions;
using MarketService_Application.Interfaces.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketService_Application.Features.Commands;

public record DeleteMarketCommand(long MarketId) : IRequest<bool>;

public class DeleteMarketCommandHandler : IRequestHandler<DeleteMarketCommand, bool>
{
    private readonly IMarketServiceDbContext _dbContext;
    private readonly ILogger<DeleteMarketCommandHandler> _logger;

    public DeleteMarketCommandHandler(IMarketServiceDbContext dbContext, ILogger<DeleteMarketCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<bool> Handle(DeleteMarketCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting market with ID: {MarketId}", request.MarketId);

        var market =
            await _dbContext.Markets.FirstOrDefaultAsync(c => c.Id == request.MarketId, cancellationToken);

        if (market == null)
        {
            _logger.LogWarning("Market with ID {MarketId} not found", request.MarketId);
            throw new MarketNotFoundException(request.MarketId);
        }
        
        _dbContext.Markets.Remove(market);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Market deleted successfully. ID: {MarketId}, Name: {MarketName}",
            request.MarketId, market.Name);
        
        return true;
    }
}