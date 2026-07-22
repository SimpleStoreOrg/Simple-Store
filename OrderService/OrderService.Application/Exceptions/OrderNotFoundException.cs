using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class OrderNotFoundException : BaseException
{
    public OrderNotFoundException(long id) 
        : base($"Order with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}