using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class CategoryNotFoundException : BaseException
{
    public CategoryNotFoundException(long id) 
        : base($"Category with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}