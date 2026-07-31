using FluentValidation;
using UserService.Application.DTOs.Request;

namespace UserService.Application.Features.Authentications.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x=>x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(20).WithMessage("Username cannot exceed 20");

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

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");
    }
}