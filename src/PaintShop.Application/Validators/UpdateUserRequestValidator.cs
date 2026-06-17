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
    }
}
