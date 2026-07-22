using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class CustomerNotFoundException : BaseException
{
    public CustomerNotFoundException(long id) : base($"Customer with Id {id} not found", StatusCodes.Status404NotFound)
    {
    }
}