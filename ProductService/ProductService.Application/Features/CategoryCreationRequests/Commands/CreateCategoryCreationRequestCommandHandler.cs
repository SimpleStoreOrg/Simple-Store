using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.Application.Features.CategoryCreationRequests.Commands;

public record CreateCategoryCreationRequestCommand(CreateCategoryRequestRequest Request)
    : IRequest<CreateCategoryRequestResponse>;

public class CreateCategoryCreationRequestCommandHandler : IRequestHandler<CreateCategoryCreationRequestCommand,
    CreateCategoryRequestResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<CreateCategoryCreationRequestCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public CreateCategoryCreationRequestCommandHandler(
        IProductServiceDbContext dbContext,
        ILogger<CreateCategoryCreationRequestCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task<CreateCategoryRequestResponse> Handle(CreateCategoryCreationRequestCommand request,
        CancellationToken cancellationToken)

    {
        var name = request.Request.Name.Trim().ToLower();
        
        var categoryExists = await _dbContext.Categories.AnyAsync(
            c => c.Name.Trim().ToLower() == name && 
                 c.ParentCategoryId == request.Request.ParentCategoryId, cancellationToken: cancellationToken);
        
        if (categoryExists)
        {
            throw new CategoryAlreadyExistsException();
        }

        var pendingRequest = await _dbContext.CategoryCreationRequests.AnyAsync(
            c => c.Name.Trim().ToLower() == name &&
                 c.ParentCategoryId == request.Request.ParentCategoryId &&
                 c.Status == CategoryRequestStatus.Pending, cancellationToken);
        
        if (pendingRequest)
        {
            throw new PendingCategoryRequestException("Requested category is pending to approve");
        }

        if (request.Request.ParentCategoryId.HasValue)
        {
            var parentCategoryExists = await _dbContext.Categories.AnyAsync(
                c => c.Id == request.Request.ParentCategoryId.Value, cancellationToken);
            
            if (!parentCategoryExists)
            {
                throw new CategoryNotFoundException(request.Request.ParentCategoryId.Value);
            }
        }

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;
        if (adminPosition != "MarketAdmin")
        {
            throw new NotAuthorizedException("Only MarketAdmins can request");
        }
        
        var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;
        if (marketIdStr == null)
        {
            throw new MarketNotFoundException();
        }

        long marketId = long.Parse(marketIdStr);

        var marketAdminIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (marketAdminIdStr == null)
        {
            throw new NotAuthorizedException("Market Admin is not authorized");
        }

        long marketAdminId = long.Parse(marketAdminIdStr);

        var createCategory = new CategoryCreationRequestEntity
        {
            Name = request.Request.Name,
            ParentCategoryId = request.Request.ParentCategoryId,
            MarketId = marketId,
            MarketAdminId = marketAdminId,
            Status = CategoryRequestStatus.Pending
        };

        await _dbContext.CategoryCreationRequests.AddAsync(createCategory, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Category creation request {RequestId} requested by MarketAdmin {AdminId}",
            createCategory.Id, marketAdminId);

        return new CreateCategoryRequestResponse
        {
            Id = createCategory.Id,
            Name = createCategory.Name,
            ParentCategoryId = createCategory.ParentCategoryId,
            MarketId = createCategory.MarketId,
            MarketAdminId = createCategory.MarketAdminId,
            Status = createCategory.Status,
            ReviewedByAdminId = createCategory.ReviewedByAdminId,
            ReviewedAt = createCategory.ReviewedAt,
            RejectionReason = createCategory.RejectionReason,
            CreatedAt = createCategory.CreatedAt
        };
    }
}