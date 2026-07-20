using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class ShopperAssistantAlreadyExistsException : BaseException
{
    public ShopperAssistantAlreadyExistsException(): base("Shopper Assistant already exists", StatusCodes.Status400BadRequest)
    {
    }
}