using FluentValidation;
using OrderService.Application.DTOs.Request;

namespace OrderService.Application.Features.Carts.Validators;

public class CreateCartRequestValidator : AbstractValidator<CreateCartRequest>
{
    public CreateCartRequestValidator()
    {
        RuleForEach(x => x.CartItems).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Order must have at least 1 product");
            
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        });
    }
}