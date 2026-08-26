using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class AdminAlreadyExistsException : BaseException
{
    public AdminAlreadyExistsException()  : base("Admin already exists", StatusCodes.Status400BadRequest)
    {
    }
}