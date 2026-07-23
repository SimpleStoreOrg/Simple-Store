using FluentValidation;
using OrderService.Application.DTOs.Request;

namespace OrderService.Application.Features.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer Id must be greater than 0");

        RuleFor(x => x.ShopperAssistantId).GreaterThan(0)
            .WithMessage("Shopper Assistant Id must be greater than 0");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Product Id must be greater than 0");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
        });
    }
}