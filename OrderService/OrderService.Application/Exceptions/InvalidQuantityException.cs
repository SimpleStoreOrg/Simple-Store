using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class InvalidQuantityException : BaseException
{
    public InvalidQuantityException(long productId) : base($"Invalid quantity for product: {productId}",
        StatusCodes.Status400BadRequest)

    {
        
    }
}