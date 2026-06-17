using FluentValidation;
using PaintShop.Application.DTOs.Inventory;

namespace PaintShop.Application.Validators;

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0);

        RuleFor(x => x.Location)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.QuantityChange)
            .NotEqual(0);
    }
}
