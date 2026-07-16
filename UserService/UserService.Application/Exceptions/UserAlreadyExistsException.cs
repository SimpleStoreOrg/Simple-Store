using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class UserAlreadyExistsException : BaseException
{
    public UserAlreadyExistsException() : base("User already exists", StatusCodes.Status400BadRequest)
    {
    }
}