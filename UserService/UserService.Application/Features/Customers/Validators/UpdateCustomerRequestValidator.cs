using FluentValidation;
using UserService.Application.DTOs.Request;

namespace UserService.Application.Features.Customers.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateUserRequest>
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

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Number is required");
    }
}