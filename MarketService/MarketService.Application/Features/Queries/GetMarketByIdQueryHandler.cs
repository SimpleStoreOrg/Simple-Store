using MarketService_Application.DTOs.Response;
using MarketService_Application.Exceptions;
using MarketService_Application.Interfaces.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketService_Application.Features.Queries;

public record GetMarketByIdQuery(long MarketId) : IRequest<MarketResponse>;

public class GetMarketByIdQueryHandler : IRequestHandler<GetMarketByIdQuery, MarketResponse>
{
    private readonly IMarketServiceDbContext _dbContext;
    private readonly ILogger<GetMarketByIdQueryHandler> _logger;

    public GetMarketByIdQueryHandler(IMarketServiceDbContext dbContext, ILogger<GetMarketByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<MarketResponse> Handle(GetMarketByIdQuery request, CancellationToken cancellationToken)
    {
        var market = await _dbContext.Markets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MarketId, cancellationToken);

        if (market == null)
        {
            _logger.LogWarning("Market with ID {MarketId} Not Found", request.MarketId);
            throw new MarketNotFoundException(request.MarketId);
        }

        return new MarketResponse
        {
            Id = market.Id,
            MarketAdminId = market.MarketAdminId,
            Name = market.Name,
            Location = market.Location,
            Email = market.Email,
            PhoneNumber = market.PhoneNumber,
            CreatedAt = market.CreatedAt,
            UpdatedAt = market.UpdatedAt,
            DeletedAt = market.DeletedAt
        };
    }
}