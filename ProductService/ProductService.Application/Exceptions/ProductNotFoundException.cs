using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class ProductNotFoundException : BaseException
{
    public ProductNotFoundException(long id) 
        : base($"Product with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}