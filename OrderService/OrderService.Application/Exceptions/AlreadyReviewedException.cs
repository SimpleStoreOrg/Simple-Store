using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class AlreadyReviewedException : BaseException
{
    public AlreadyReviewedException() : base("You have already reviewed this product",
        StatusCodes.Status406NotAcceptable)

    {
    }
}