using FluentValidation;
using PaintShop.Application.DTOs.Inventory;

namespace PaintShop.Application.Validators;

public class AddStockRequestValidator : AbstractValidator<AddStockRequest>
{
    public AddStockRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0);

        RuleFor(x => x.Location)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
