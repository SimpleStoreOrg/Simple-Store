using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class ProductAlreadyExistsException : BaseException
{
    public ProductAlreadyExistsException() : base("Product already exists", StatusCodes.Status400BadRequest)
    {
    }
}