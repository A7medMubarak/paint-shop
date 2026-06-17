using FluentValidation;
using PaintShop.Application.DTOs.Products;

namespace PaintShop.Application.Validators;

public class CreateProductVariantRequestValidator : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantRequestValidator()
    {
        RuleFor(x => x.SizeValue)
            .GreaterThan(0);

        RuleFor(x => x.SizeUnit)
            .NotEmpty()
            .MaximumLength(10);

        When(x => x.BaseType.HasValue, () =>
        {
            RuleFor(x => x.BaseType!.Value)
                .InclusiveBetween(0, 5);
        });

        When(x => x.SellingPrice.HasValue, () =>
        {
            RuleFor(x => x.SellingPrice!.Value)
                .GreaterThanOrEqualTo(0);
        });

        When(x => x.CostPrice.HasValue, () =>
        {
            RuleFor(x => x.CostPrice!.Value)
                .GreaterThanOrEqualTo(0);
        });

        When(x => x.LowStockThreshold.HasValue, () =>
        {
            RuleFor(x => x.LowStockThreshold!.Value)
                .GreaterThanOrEqualTo(0);
        });
    }
}
