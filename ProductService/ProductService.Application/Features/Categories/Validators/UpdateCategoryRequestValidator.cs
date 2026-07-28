using FluentValidation;
using ProductService.Application.DTOs.Request;

namespace ProductService.Application.Features.Categories.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(15)
            .WithMessage("Name cannot exceed 15");
    }
}