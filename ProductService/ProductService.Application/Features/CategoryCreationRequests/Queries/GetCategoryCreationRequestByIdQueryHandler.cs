using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.CategoryCreationRequests.Queries;

public record GetCategoryCreationRequestByIdQuery(long Id) : IRequest<CreateCategoryRequestResponse>;

public class GetCategoryCreationRequestByIdQueryHandler : IRequestHandler<GetCategoryCreationRequestByIdQuery,
    CreateCategoryRequestResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<GetCategoryCreationRequestByIdQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public GetCategoryCreationRequestByIdQueryHandler(
        IProductServiceDbContext dbContext,
        ILogger<GetCategoryCreationRequestByIdQueryHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<CreateCategoryRequestResponse> Handle(GetCategoryCreationRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;
        
        var query = _dbContext.CategoryCreationRequests.AsNoTracking().AsQueryable();

        if (role == "Admin" && adminPosition == "MarketAdmin")
        {
            var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;

            if (marketIdStr == null)
            {
                throw new NotAuthorizedException("Market ID not found.");
            }

            long marketId = long.Parse(marketIdStr);
            
            query = query.Where(c => c.MarketId == marketId);
        }

        var requestedCategory = await query.FirstOrDefaultAsync(cancellationToken);
        if (requestedCategory == null)
        {
            _logger.LogWarning("Category request with ID {CategoryId} not found", request.Id);
            throw new CategoryNotFoundException(request.Id);
        }

        return new CreateCategoryRequestResponse
        {
            Id = requestedCategory.Id,
            Name = requestedCategory.Name,
            ParentCategoryId = requestedCategory.ParentCategoryId,
            MarketId = requestedCategory.MarketId,
            MarketAdminId = requestedCategory.MarketAdminId,
            Status = requestedCategory.Status,
            ReviewedByAdminId = requestedCategory.ReviewedByAdminId,
            ReviewedAt = requestedCategory.ReviewedAt,
            RejectionReason = requestedCategory.RejectionReason,
            CreatedAt = requestedCategory.CreatedAt,
            UpdatedAt = requestedCategory.UpdatedAt,
            DeletedAt = requestedCategory.DeletedAt
        };
    }
}