using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;

namespace OrderService.Application.Features.Queries;

public record GetMarketCustomerIdsQuery : IRequest<List<long>>;

public class GetMarketCustomerIdsQueryHandler : IRequestHandler<GetMarketCustomerIdsQuery, List<long>>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly IHttpContextAccessor _accessor;

    public GetMarketCustomerIdsQueryHandler(IOrderServiceDbContext dbContext, IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }
    public async Task<List<long>> Handle(GetMarketCustomerIdsQuery request, CancellationToken cancellationToken)
    {
        var marketIdStr = _accessor.HttpContext?.User.FindFirst("MarketId")?.Value;
        
        if (marketIdStr == null)
        {
            throw new NotAuthorizedException("Market is not authorized");
        }

        long marketId = long.Parse(marketIdStr);
        
        return await _dbContext.Orders.AsNoTracking()
            .Where(o => o.OrderItems.Any(oi => oi.MarketId == marketId))
            .Select(o=>o.CustomerId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}