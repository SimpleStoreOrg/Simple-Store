using FluentValidation;
using UserService.Application.DTOs.Request;

namespace UserService.Application.Features.ShopperAssistants.Validators;

public class CreateShopperAssistantRequestValidator : AbstractValidator<CreateShopperAssistantRequest>
{
    public CreateShopperAssistantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(20).WithMessage("Name cannot exceed 20");
        
        RuleFor(x=>x.Surname)
            .NotEmpty().WithMessage("Surname is required")
            .MaximumLength(20).WithMessage("Surname cannot exceed 20");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Wrong email fromat");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(20).WithMessage("Password cannot exceed 20 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Number is required");

        RuleFor(x => x.Position)
            .IsInEnum().WithMessage("Invalid shopper assistant position");
    }
}