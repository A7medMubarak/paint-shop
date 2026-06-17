using FluentValidation;
using PaintShop.Application.DTOs.Products;

namespace PaintShop.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ProductCategory)
            .InclusiveBetween(0, 2);
    }
}
