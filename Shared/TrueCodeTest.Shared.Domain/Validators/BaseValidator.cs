using FluentValidation;

namespace TrueCodeTest.Shared.Domain.Validators;

public abstract class BaseValidator<T> : AbstractValidator<T>
{
    protected const int MAX_USERNAME_LENGTH = 50;
    protected const int MIN_PASSWORD_LENGTH = 6;
    protected const int MAX_PASSWORD_LENGTH = 100;
    protected const int MAX_CURRENCY_NAME_LENGTH = 100;

    protected void ValidateUserName(System.Linq.Expressions.Expression<Func<T, string>> expression, string fieldName = "Имя пользователя")
    {
        RuleFor(expression)
            .NotEmpty()
            .WithMessage($"{fieldName} не может быть пустым")
            .MaximumLength(MAX_USERNAME_LENGTH)
            .WithMessage($"{fieldName} не может превышать {MAX_USERNAME_LENGTH} символов");
    }

    protected void ValidatePassword(System.Linq.Expressions.Expression<Func<T, string>> expression, string fieldName = "Пароль")
    {
        RuleFor(expression)
            .NotEmpty()
            .WithMessage($"{fieldName} не может быть пустым")
            .MinimumLength(MIN_PASSWORD_LENGTH)
            .WithMessage($"{fieldName} должен содержать минимум {MIN_PASSWORD_LENGTH} символов")
            .MaximumLength(MAX_PASSWORD_LENGTH)
            .WithMessage($"{fieldName} не может превышать {MAX_PASSWORD_LENGTH} символов");
    }

    protected void ValidateUserId(System.Linq.Expressions.Expression<Func<T, int>> expression, string fieldName = "UserId")
    {
        RuleFor(expression)
            .GreaterThan(0)
            .WithMessage($"{fieldName} должен быть больше 0");
    }

    protected void ValidateCurrencyName(System.Linq.Expressions.Expression<Func<T, string>> expression, string fieldName = "Название валюты")
    {
        RuleFor(expression)
            .NotEmpty()
            .WithMessage($"{fieldName} не может быть пустым")
            .MaximumLength(MAX_CURRENCY_NAME_LENGTH)
            .WithMessage($"{fieldName} не может превышать {MAX_CURRENCY_NAME_LENGTH} символов");
    }
}
