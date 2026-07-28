using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.ShopperAssistants.Queries;

public record GetAllShopperAssistantsQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<ShopperAssistantResponse>>;

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
        
        if (request.PageNumber <= 0 || request.PageSize <= 0)
        {
            _logger.LogWarning("Invalid pagination parameters. Page: {PageNumber}, Size: {PageSize}",
                request.PageNumber, request.PageSize);
            return new PagedResponse<ShopperAssistantResponse>();
        }

        var query = _dbContext.ShopperAssistants.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var shopperAssistants = await query
            .OrderBy(e => e.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new ShopperAssistantResponse()
            {
                Id = e.Id,
                Name = e.Name,
                Surname = e.Surname,
                Role = e.Role,
                Position = e.Position,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToListAsync(cancellationToken);
        
        _logger.LogInformation("Returned {Count} Shopper Assistants out of {Total}", shopperAssistants.Count, totalCount);
        
        return new PagedResponse<ShopperAssistantResponse>()
        {
            Items = shopperAssistants,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}