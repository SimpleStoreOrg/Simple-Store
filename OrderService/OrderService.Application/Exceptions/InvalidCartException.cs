using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class InvalidCartException : BaseException
{
    public InvalidCartException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}