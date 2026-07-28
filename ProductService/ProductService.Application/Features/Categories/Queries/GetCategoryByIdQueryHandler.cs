using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs.Response;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.Data;

namespace ProductService.Application.Features.Categories.Queries;

public record GetCategoryByIdQuery(long CategoryId) : IRequest<CategoryResponse>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponse>
{
    private readonly IProductServiceDbContext _dbContext;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(IProductServiceDbContext dbContext, ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<CategoryResponse> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category with ID {CustomerId} not found", request.CategoryId);
            throw new CategoryNotFoundException(request.CategoryId);
        }

        return new CategoryResponse()
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            DeletedAt = category.DeletedAt //Chek them
        };
    }
}