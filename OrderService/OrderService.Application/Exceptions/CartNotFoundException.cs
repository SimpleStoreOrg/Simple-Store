using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class CartNotFoundException : BaseException
{
    public CartNotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}