using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class IncorrectPaginationException : BaseException
{
    public IncorrectPaginationException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}