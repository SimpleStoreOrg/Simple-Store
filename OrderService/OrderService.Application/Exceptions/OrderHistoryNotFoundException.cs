using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class OrderHistoryNotFoundException : BaseException
{
    public OrderHistoryNotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}