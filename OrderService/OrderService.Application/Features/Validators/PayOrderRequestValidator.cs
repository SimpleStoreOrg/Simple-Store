using FluentValidation;
using OrderService.Application.DTOs.Request;

namespace OrderService.Application.Features.Validators;

public class PayOrderRequestValidator : AbstractValidator<PayOrderRequest>
{
    public PayOrderRequestValidator()
    {
        RuleFor(x => x.AmountPaid).GreaterThan(0).WithMessage("Payment amount must be greater than 0");
    }
}