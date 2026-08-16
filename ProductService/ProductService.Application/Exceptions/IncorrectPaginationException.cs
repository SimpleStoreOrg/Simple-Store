using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class IncorrectPaginationException : BaseException
{
    public IncorrectPaginationException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}