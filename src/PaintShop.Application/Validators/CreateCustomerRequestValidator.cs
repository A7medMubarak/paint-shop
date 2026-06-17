using FluentValidation;
using PaintShop.Application.DTOs.Customers;

namespace PaintShop.Application.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(20);
    }
}
