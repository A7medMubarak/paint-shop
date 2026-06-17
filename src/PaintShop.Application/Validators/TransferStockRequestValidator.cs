using FluentValidation;
using PaintShop.Application.DTOs.Inventory;

namespace PaintShop.Application.Validators;

public class TransferStockRequestValidator : AbstractValidator<TransferStockRequest>
{
    public TransferStockRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.FromLocation)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.ToLocation)
            .InclusiveBetween(0, 1);

        RuleFor(x => x)
            .Must(x => x.FromLocation != x.ToLocation)
            .WithMessage("Source and destination locations cannot be the same");
    }
}
