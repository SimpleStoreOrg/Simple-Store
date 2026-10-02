using Microsoft.AspNetCore.Http;

namespace MarketService_Application.Exceptions;

public class NotAuthorizedException : BaseException
{
    public NotAuthorizedException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}