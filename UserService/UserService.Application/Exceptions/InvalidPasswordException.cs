using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class InvalidPasswordException : BaseException
{
    public InvalidPasswordException() 
        : base("Invalid password", StatusCodes.Status400BadRequest)
    {
    }
}