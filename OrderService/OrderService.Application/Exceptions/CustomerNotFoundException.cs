using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class CustomerNotFoundException : BaseException
{
    public CustomerNotFoundException(long id) 
        : base($"Customer with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}