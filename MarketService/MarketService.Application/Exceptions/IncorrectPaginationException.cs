using Microsoft.AspNetCore.Http;

namespace MarketService_Application.Exceptions;

public class IncorrectPaginationException : BaseException
{
    public IncorrectPaginationException(string message) : base(message,StatusCodes.Status400BadRequest)
    {
    }
}