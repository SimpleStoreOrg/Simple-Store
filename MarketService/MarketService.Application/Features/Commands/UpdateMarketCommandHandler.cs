using MarketService_Application.DTOs.Request;
using MarketService_Application.DTOs.Response;
using MarketService_Application.Exceptions;
using MarketService_Application.Interfaces.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketService_Application.Features.Commands;

public record UpdateMarketCommand(long MarketId, UpdateMarketRequest Request) : IRequest<MarketResponse>;

public class UpdateMarketCommandHandler : IRequestHandler<UpdateMarketCommand, MarketResponse>
{
    private readonly IMarketServiceDbContext _dbContext;
    private readonly ILogger<UpdateMarketCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public UpdateMarketCommandHandler(
        IMarketServiceDbContext dbContext,
        ILogger<UpdateMarketCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    
    public async Task<MarketResponse> Handle(UpdateMarketCommand request, CancellationToken cancellationToken)
    {
        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        if (adminPosition == "MarketAdmin")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;
            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID was not found.");
            }

            long marketId = long.Parse(marketIdStr);
            if (marketId != request.MarketId)
            {
                throw new NotAuthorizedException("You can only update your own market.");
            }
        }
        
        var exists = await _dbContext.Markets.AnyAsync(
            m => m.Id != request.MarketId && m.Name!.Trim().ToLower() == request.Request.Name.Trim().ToLower(),
            cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Market already exists with name: {Name}", request.Request.Name);
            throw new MarketAlreadyExistsException(request.Request.Name);
        }

        var market = await _dbContext.Markets.FirstOrDefaultAsync(m => m.Id == request.MarketId,
            cancellationToken);
        
        if (market == null)
        {
            _logger.LogWarning("Market with ID {MarketId} not found", request.MarketId);
            throw new MarketNotFoundException(request.MarketId);
        }

        market.Name = request.Request.Name;
        market.Location = request.Request.Location;
        market.Email = request.Request.Email;
        market.PhoneNumber = request.Request.PhoneNumber;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new MarketResponse
        {
            Id = market.Id,
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