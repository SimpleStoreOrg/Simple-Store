using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class UserNotFoundException : BaseException
{
    public UserNotFoundException(long id) : base($"User with Id {id} not found", StatusCodes.Status404NotFound)
    {
    }
}