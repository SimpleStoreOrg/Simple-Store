using FluentValidation;
using OrderService.Application.DTOs.Request;

namespace OrderService.Application.Features.Validators;

public class AssignOrderRequestValidator : AbstractValidator<AssignOrderRequest>
{
    public AssignOrderRequestValidator()
    {
        RuleFor(x => x.ShopperAssistantId).GreaterThan(0)
            .WithMessage("Shopper Assistant Id must be greater than 0");
    }
}