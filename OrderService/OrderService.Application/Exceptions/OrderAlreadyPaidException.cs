using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class OrderAlreadyPaidException : BaseException
{
    public OrderAlreadyPaidException(long id) : base($"Order with ID {id} is already paid",
        StatusCodes.Status406NotAcceptable)

    {
    }
}