using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;

namespace ProductService.Application.Features.CategoryCreationRequests.Commands;

public record ApproveCategoryCreationRequestCommand(long Id) : IRequest<CategoryResponse>;

public class ApproveCategoryCreationRequestCommandHandler : IRequestHandler<ApproveCategoryCreationRequestCommand,
    CategoryResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<ApproveCategoryCreationRequestCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public ApproveCategoryCreationRequestCommandHandler(
        IProductServiceDbContext dbContext,
        ILogger<ApproveCategoryCreationRequestCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }
    public async Task<CategoryResponse> Handle(ApproveCategoryCreationRequestCommand request, CancellationToken cancellationToken)
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
            throw new NotAuthorizedException("Only SuperAdmin can request");
        }
        
        var superAdminIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (superAdminIdStr == null)
        {
            throw new NotAuthorizedException("Admin is not authorized");
        }

        long superAdminId = long.Parse(superAdminIdStr);

        var name = categoryRequest.Name.Trim().ToLower();
        var exists = await _dbContext.Categories.AnyAsync(
            c => c.Name.Trim().ToLower() == name && c.ParentCategoryId == categoryRequest.ParentCategoryId,
            cancellationToken);

        if (exists)
        {
            throw new CategoryAlreadyExistsException();
        }
        
        if (categoryRequest.ParentCategoryId.HasValue)
        {
            var parentExists = await _dbContext.Categories.AnyAsync(
                c => c.Id == categoryRequest.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
            {
                throw new CategoryNotFoundException(categoryRequest.ParentCategoryId.Value);
            }
        }
        
        var category = new CategoryEntity
        {
            Name = categoryRequest.Name,
            ParentCategoryId = categoryRequest.ParentCategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Categories.AddAsync(category, cancellationToken);
        
        categoryRequest.Status = CategoryRequestStatus.Approved;
        categoryRequest.ReviewedByAdminId = superAdminId;
        categoryRequest.ReviewedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category creation request {RequestId} approved by SuperAdmin {AdminId}",
            categoryRequest.Id, superAdminId);

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            DeletedAt = category.DeletedAt
        };
    }
}