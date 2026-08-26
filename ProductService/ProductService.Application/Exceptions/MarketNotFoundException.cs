using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class MarketNotFoundException : BaseException
{
    public MarketNotFoundException(long id) : base($"Market with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}