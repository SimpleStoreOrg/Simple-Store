using Microsoft.AspNetCore.Http;

namespace OrderService.Application.Exceptions;

public class InsufficientPaymentException : BaseException
{
    public InsufficientPaymentException(decimal amountPaid, decimal total) : base(
        $"Not enough money: {amountPaid}. The total price: {total}", StatusCodes.Status400BadRequest)

    {
        
    }
}