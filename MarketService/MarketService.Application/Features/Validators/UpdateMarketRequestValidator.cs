using FluentValidation;
using MarketService_Application.DTOs.Request;

namespace MarketService_Application.Features.Validators;

public class UpdateMarketRequestValidator : AbstractValidator<UpdateMarketRequest>
{
    public UpdateMarketRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(50)
            .WithMessage("Name cannot exceed 50");
        
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required").MaximumLength(50)
            .WithMessage("Location cannot exceed 50");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Wrong email fromat");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Number is required");
    }
}