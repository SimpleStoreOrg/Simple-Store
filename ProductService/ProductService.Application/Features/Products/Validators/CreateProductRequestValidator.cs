using FluentValidation;
using ProductService.Application.DTOs.Request;

namespace ProductService.Application.Features.Products.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(20).WithMessage("Name cannot exceed 20");
        
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category must be greater than 0");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");
        
        RuleFor(x=>x.Stock)
            .GreaterThan(0).WithMessage("Stock must be greater than 0");
    }
}