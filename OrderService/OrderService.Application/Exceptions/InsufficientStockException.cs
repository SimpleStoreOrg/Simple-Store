using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class InsufficientStockException : BaseException
{
    public InsufficientStockException(long productId, decimal available, decimal requested) 
        : base(
            $"Not enough stock for product {productId}. Available: {available}, Requested: {requested}",
            StatusCodes.Status400BadRequest)

    {
        
    }
}