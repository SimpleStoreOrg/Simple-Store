using FluentValidation;
using UserService.Application.DTOs.Request;

namespace UserService.Application.Features.Customers.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
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

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(20).WithMessage("Username cannot exceed 20")
            .Matches("[A-Z]").WithMessage("Username must contain at least 1 uppercase")
            .Matches("[a-z]").WithMessage("Username must contain at least 1 lowercase");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Number is required");
    }
}