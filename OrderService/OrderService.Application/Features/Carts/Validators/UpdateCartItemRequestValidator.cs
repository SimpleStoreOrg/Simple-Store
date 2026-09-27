using FluentValidation;
using OrderService.Application.DTOs.Request;

namespace OrderService.Application.Features.Carts.Validators;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x=>x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}