using Microsoft.AspNetCore.Http;

namespace MarketService_Application.Exceptions;

public class MarketAlreadyExistsException : BaseException
{
    public MarketAlreadyExistsException(string? marketName) : base($"Market with name {marketName} already exists",
        StatusCodes.Status400BadRequest)

    {
    }
}