using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class CustomerAlreadyExistsException : BaseException
{
    public CustomerAlreadyExistsException() : base("Customer already exists", StatusCodes.Status400BadRequest)
    {
    }
}