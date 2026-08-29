using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class NotAuthorizedException : BaseException
{
    public NotAuthorizedException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}