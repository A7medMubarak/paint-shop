using FluentValidation;
using PaintShop.Application.DTOs.Sales;

namespace PaintShop.Application.Validators;

public class CreateSaleItemRequestValidator : AbstractValidator<CreateSaleItemRequest>
{
    public CreateSaleItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);
    }
}
