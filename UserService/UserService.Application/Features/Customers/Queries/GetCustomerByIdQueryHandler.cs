using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Response;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces.Data;
using UserService.Application.Interfaces.External;

namespace UserService.Application.Features.Customers.Queries;

public record GetCustomerByIdQuery(long CustomerId) : IRequest<CustomerResponse>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponse>
{
    private readonly IUserServiceDbContext _dbContext;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly IOrderApi _orderApi;

    public GetCustomerByIdQueryHandler(
        IUserServiceDbContext dbContext,
        ILogger<GetCustomerByIdQueryHandler> logger,
        IHttpContextAccessor accessor,
        IOrderApi orderApi)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
        _orderApi = orderApi;
    }
    
    public async Task<CustomerResponse> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        var adminPosition = _accessor.HttpContext?.User.FindFirst("AdminPosition")?.Value;

        var query = _dbContext.Customers.AsNoTracking()
            .Where(c => c.Id == request.CustomerId);

        if ((role == "Admin" && adminPosition == "MarketAdmin") || role == "ShopperAssistant")
        {
            var token = _accessor.HttpContext?.Request.Headers["Authorization"].ToString();

            var customerIds = await _orderApi.GetMarketCustomerIds(token);

            if (!customerIds.Contains(request.CustomerId))
            {
                throw new CustomerNotFoundException(request.CustomerId);
            }
        }

        var customer = await query.FirstOrDefaultAsync(cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            throw new CustomerNotFoundException(request.CustomerId);
        }

        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Surname = customer.Surname,
            Role = customer.Role,
            Username = customer.UserName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            DeletedAt = customer.DeletedAt
        };
    }
}