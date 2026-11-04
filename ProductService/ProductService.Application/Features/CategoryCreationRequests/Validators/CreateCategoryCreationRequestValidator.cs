using FluentValidation;
using ProductService.Application.DTOs.Request;

namespace ProductService.Application.Features.CategoryCreationRequests.Validators;

public class CreateCategoryCreationRequestValidator : AbstractValidator<CreateCategoryRequestRequest>
{
    public CreateCategoryCreationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");
    }
}