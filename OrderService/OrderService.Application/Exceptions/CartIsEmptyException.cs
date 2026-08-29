using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class CartIsEmptyException : BaseException
{
    public CartIsEmptyException() : base("Cart is empty", StatusCodes.Status400BadRequest)
    {
    }
}