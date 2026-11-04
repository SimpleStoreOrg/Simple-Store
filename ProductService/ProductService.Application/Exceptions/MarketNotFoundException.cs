using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class MarketNotFoundException : BaseException
{
    public MarketNotFoundException() : base($"Market not found", StatusCodes.Status404NotFound)
    {
    }
}