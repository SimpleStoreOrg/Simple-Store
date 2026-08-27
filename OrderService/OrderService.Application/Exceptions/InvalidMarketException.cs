using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class InvalidMarketException : BaseException
{
    public InvalidMarketException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}