using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class ShopperAssistantNotFoundException : BaseException
{
    public ShopperAssistantNotFoundException(long id) 
        : base($"Shopper Assistant with ID {id} not found", StatusCodes.Status404NotFound)
    {
    }
}