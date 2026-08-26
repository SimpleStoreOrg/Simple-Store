using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Exceptions;

public class InvalidMarketException : BaseException
{
    public InvalidMarketException(long marketId) : base(
        $"The product does not belong to this Market with ID: {marketId}", StatusCodes.Status400BadRequest)

    {
    }
}