using FluentValidation;
using PaintShop.Application.DTOs.Products;

namespace PaintShop.Application.Validators;

public class UpdateProductVariantRequestValidator : AbstractValidator<UpdateProductVariantRequest>
{
    public UpdateProductVariantRequestValidator()
    {
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
