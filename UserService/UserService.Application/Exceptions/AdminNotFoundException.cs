using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class AdminNotFoundException : BaseException
{
    public AdminNotFoundException(long id) : base($"Admin with Id {id} not found", StatusCodes.Status404NotFound)
    {
    }
}