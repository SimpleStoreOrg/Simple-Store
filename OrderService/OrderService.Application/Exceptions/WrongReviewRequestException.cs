using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class WrongReviewRequestException : BaseException
{
    public WrongReviewRequestException() : base(
        "You cannot review this product because it was not purchased in this order", StatusCodes.Status400BadRequest)

    {
    }
}