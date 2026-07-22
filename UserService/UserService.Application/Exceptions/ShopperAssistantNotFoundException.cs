using Microsoft.AspNetCore.Http;

namespace UserService.Application.Exceptions;

public class ShopperAssistantNotFoundException : BaseException
{
    public ShopperAssistantNotFoundException(long id) : base($"Shopper Assistant with Id {id} not found", StatusCodes.Status404NotFound)
    {
    }
}