using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class CategoryRequestAlreadyReviewedException : BaseException
{
    public CategoryRequestAlreadyReviewedException() : base("This request has already reviewed",
        StatusCodes.Status400BadRequest)

    {
    }
}