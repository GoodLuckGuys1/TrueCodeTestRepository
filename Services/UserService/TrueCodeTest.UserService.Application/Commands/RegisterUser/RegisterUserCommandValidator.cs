using FluentValidation;
using TrueCodeTest.Shared.Domain.Validators;

namespace TrueCodeTest.UserService.Application.Commands.RegisterUser;

public class RegisterUserCommandValidator : BaseValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Имя пользователя не может быть пустым")
            .MaximumLength(MAX_USERNAME_LENGTH)
            .WithMessage($"Имя пользователя не может превышать {MAX_USERNAME_LENGTH} символов")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Имя пользователя может содержать только латинские буквы, цифры и подчеркивания");

        ValidatePassword(x => x.Password);
    }
}
