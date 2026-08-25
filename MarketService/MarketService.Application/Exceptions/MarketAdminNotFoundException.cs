using Microsoft.AspNetCore.Http;

namespace MarketService_Application.Exceptions;

public class MarketAdminNotFoundException : BaseException
{
    public MarketAdminNotFoundException(long id) : base($"Market Admin with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}