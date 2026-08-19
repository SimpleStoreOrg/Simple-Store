using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;

namespace UserService.Application.Features.Customers.Queries;

public record GetAllCustomersQuery(
    int? PageNumber = null,
    int? PageSize = null) : IRequest<PagedResponse<CustomerResponse>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PagedResponse<CustomerResponse>>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetAllCustomersQueryHandler> _logger;

    public GetAllCustomersQueryHandler(IUserServiceDbContext dbContext, ILogger<GetAllCustomersQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<PagedResponse<CustomerResponse>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching Customers. Page: {PageNumber}, Size: {PageSize}", request.PageNumber, request.PageSize);
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
        
        var query = _dbContext.Customers.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            query = query
                .OrderBy(c => c.Id)
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);
        }

        var customers = await query
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Surname = c.Surname,
                Role = c.Role,
                Username = c.UserName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                DeletedAt = c.DeletedAt
            }).ToListAsync(cancellationToken);
        
        _logger.LogInformation("Returned {Count} customers out of {Total}", customers.Count, totalCount);
        
        return new PagedResponse<CustomerResponse>
        {
            Items = customers,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}