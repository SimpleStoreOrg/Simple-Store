using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class DuplicateProductException : BaseException
{
    public DuplicateProductException() : base("Duplicate products are not allowed in one order",
        StatusCodes.Status400BadRequest)

    {
        
    }
}