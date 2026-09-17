using FluentValidation;
using PaintShop.Application.DTOs.Users;

namespace PaintShop.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Role)
            .Must(r => r == null || r == "Owner" || r == "Employee")
            .WithMessage("Role must be Owner or Employee.")
            .When(x => x.Role != null);
    }
}
