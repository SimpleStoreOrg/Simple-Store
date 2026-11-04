using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;
using ProductService.Domain.Enums;

namespace ProductService.Application.Features.CategoryCreationRequests.Commands;

public record RejectCategoryCreationRequestCommand(long Id, string RejectReason)
    : IRequest<CreateCategoryRequestResponse>;

public class RejectCategoryCreationRequestCommandHandler : IRequestHandler<RejectCategoryCreationRequestCommand,
    CreateCategoryRequestResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<RejectCategoryCreationRequestCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public RejectCategoryCreationRequestCommandHandler(
        IProductServiceDbContext dbContext,
        ILogger<RejectCategoryCreationRequestCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<CreateCategoryRequestResponse> Handle(RejectCategoryCreationRequestCommand request, CancellationToken cancellationToken)
    {
        var categoryRequest = await _dbContext.CategoryCreationRequests.FirstOrDefaultAsync(
            c => c.Id == request.Id, cancellationToken);
        if (categoryRequest == null)
        {
            throw new CategoryNotFoundException(request.Id);
        }

        if (categoryRequest.Status != CategoryRequestStatus.Pending)
        {
            throw new CategoryRequestAlreadyReviewedException();
        }
        
        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        if (adminPosition != "SuperAdmin")
        {
            throw new NotAuthorizedException("Only SuperAdmins can reject category requests");
        }

        var superAdminIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (superAdminIdStr == null)
        {
            throw new NotAuthorizedException("Admin is not authorized");
        }

        long superAdminId = long.Parse(superAdminIdStr);

        categoryRequest.Status = CategoryRequestStatus.Rejected;
        categoryRequest.ReviewedByAdminId = superAdminId;
        categoryRequest.ReviewedAt = DateTime.UtcNow;
        categoryRequest.RejectionReason = request.RejectReason;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category creation request {RequestId} rejected by SuperAdmin {AdminId}",
            categoryRequest.Id, superAdminId);

        return new CreateCategoryRequestResponse
        {
            Id = categoryRequest.Id,
            Name = categoryRequest.Name,
            ParentCategoryId = categoryRequest.ParentCategoryId,
            MarketId = categoryRequest.MarketId,
            MarketAdminId = categoryRequest.MarketAdminId,
            Status = categoryRequest.Status,
            ReviewedByAdminId = categoryRequest.ReviewedByAdminId,
            ReviewedAt = categoryRequest.ReviewedAt,
            RejectionReason = categoryRequest.RejectionReason,
            CreatedAt = categoryRequest.CreatedAt
        };
    }
}