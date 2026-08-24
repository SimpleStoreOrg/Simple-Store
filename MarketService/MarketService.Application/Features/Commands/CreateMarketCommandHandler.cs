using MarketService_Application.DTOs.Request;
using MarketService_Application.DTOs.Response;
using MarketService_Application.Exceptions;
using MarketService_Application.Interfaces.Data;
using MarketService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketService_Application.Features.Commands;

public record CreateMarketCommand(CreateMarketRequest Request) : IRequest<MarketResponse>;
public class CreateMarketCommandHandler : IRequestHandler<CreateMarketCommand, MarketResponse>
{
    private readonly IMarketServiceDbContext _dbContext;
    private readonly ILogger<CreateMarketCommandHandler> _logger;

    public CreateMarketCommandHandler(IMarketServiceDbContext dbContext, ILogger<CreateMarketCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<MarketResponse> Handle(CreateMarketCommand request, CancellationToken cancellationToken)
    {
        var exists =
            await _dbContext.Markets.AnyAsync(m => m.Name!.Trim().ToLower() == request.Request.Name.Trim().ToLower(),
                cancellationToken);
        
        if (exists)
        {
            _logger.LogWarning("Market already exists with name: {Name}", request.Request.Name);
            throw new MarketAlreadyExistsException(request.Request.Name);
        }
        
        _logger.LogWarning("New market creation");
        var market = new MarketEntity
        {
            Name = request.Request.Name,
            Location = request.Request.Location,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber
        };
        
        await _dbContext.Markets.AddAsync(market, cancellationToken);
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