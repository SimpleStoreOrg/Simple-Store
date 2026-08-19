using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Domain.Enums;

namespace UserService.Application.Features.ShopperAssistants.Queries;

public record GetAllShopperAssistantsQuery(
    int? PageNumber = null,
    int? PageSize = null,
    ShopperAssistantPosition? Positions = null) : IRequest<PagedResponse<ShopperAssistantResponse>>;

public class GetAllShopperAssistantsQueryHandler : IRequestHandler<GetAllShopperAssistantsQuery, PagedResponse<ShopperAssistantResponse>>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetAllShopperAssistantsQueryHandler> _logger;

    public GetAllShopperAssistantsQueryHandler(IUserServiceDbContext dbContext, ILogger<GetAllShopperAssistantsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<PagedResponse<ShopperAssistantResponse>> Handle(GetAllShopperAssistantsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Shopper Assistants. Page: {PageNumber}, Size: {PageSize}", request.PageNumber,
            request.PageSize);
        if (request.PageNumber.HasValue && request.PageNumber.Value <= 0)
        {
            _logger.LogWarning("Page number {PageNumber}, must be greater than 0", request.PageNumber);
            throw new IncorrectPaginationException("Page number must be greater than 0.");
        }

        if (request.PageSize.HasValue && request.PageSize.Value <= 0)
        {
            _logger.LogWarning("Page size {PageSize}, must be greater than 0", request.PageSize);
            throw new IncorrectPaginationException("Page size must be greater than 0.");
        }

        var query = _dbContext.ShopperAssistants.AsQueryable();

        if (request.Positions.HasValue)
        {
            query = query.Where(s => s.Position == request.Positions);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var shopperAssistants = await query
            .Select(e => new ShopperAssistantResponse
            {
                Id = e.Id,
                Name = e.Name,
                Surname = e.Surname,
                Role = e.Role,
                Position = e.Position,
                Username = e.UserName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                DeletedAt = e.DeletedAt
            }).ToListAsync(cancellationToken);
        
        _logger.LogInformation("Returned {Count} Shopper Assistants out of {Total}", shopperAssistants.Count, totalCount);
        
        return new PagedResponse<ShopperAssistantResponse>
        {
            Items = shopperAssistants,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}