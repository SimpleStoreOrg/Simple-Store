using MarketService_Application.DTOs.External;
using MarketService_Application.DTOs.Request;
using MarketService_Application.DTOs.Response;
using MarketService_Application.Exceptions;
using MarketService_Application.Interfaces.Data;
using MarketService_Application.Interfaces.External;
using MarketService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketService_Application.Features.Commands;

public record CreateMarketCommand(CreateMarketRequest Request) : IRequest<MarketResponse>;
public class CreateMarketCommandHandler : IRequestHandler<CreateMarketCommand, MarketResponse>
{
    private readonly IMarketServiceDbContext _dbContext;
    private readonly ILogger<CreateMarketCommandHandler> _logger;
    private readonly IMarketAdminApi _marketAdminApi;
    private readonly IHttpContextAccessor _accessor;

    public CreateMarketCommandHandler(
        IMarketServiceDbContext dbContext,
        ILogger<CreateMarketCommandHandler> logger,
        IMarketAdminApi marketAdminApi,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _marketAdminApi = marketAdminApi;
        _accessor = accessor;
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

        var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();
        
        var marketAdmin = await _marketAdminApi.GetMarketAdminById(request.Request.MarketAdminId, token);
        
        if (marketAdmin == null)
        {
            _logger.LogWarning("Market Admin with ID {MarketAdminId} not found", request.Request.MarketAdminId);
            throw new MarketAdminNotFoundException(request.Request.MarketAdminId);
        }
        
        _logger.LogInformation("Creating new market for MarketAdmin {MarketAdminId}", marketAdmin.Id);

        var market = new MarketEntity
        {
            MarketAdminId = marketAdmin.Id,
            Name = request.Request.Name,
            Location = request.Request.Location,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber
        };
        
        await _dbContext.Markets.AddAsync(market, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Market created successfully with ID {MarketId}", market.Id);

        await _marketAdminApi.AssignMarketToAdmin(marketAdmin.Id, new AssignMarketRequest
            {
                MarketId = market.Id
            }, token);

        _logger.LogInformation("Market {MarketId} assigned to MarketAdmin {MarketAdminId}", market.Id, marketAdmin.Id);

        return new MarketResponse
        {
            Id = market.Id,
            MarketAdminId = marketAdmin.Id,
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