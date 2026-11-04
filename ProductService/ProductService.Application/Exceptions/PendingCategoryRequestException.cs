using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class PendingCategoryRequestException : BaseException
{
    public PendingCategoryRequestException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}