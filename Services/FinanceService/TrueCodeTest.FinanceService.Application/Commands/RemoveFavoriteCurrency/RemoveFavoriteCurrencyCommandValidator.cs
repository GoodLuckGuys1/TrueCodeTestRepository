using TrueCodeTest.Shared.Domain.Validators;

namespace TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;

public class RemoveFavoriteCurrencyCommandValidator : BaseValidator<RemoveFavoriteCurrencyCommand>
{
    public RemoveFavoriteCurrencyCommandValidator()
    {
        ValidateUserId(x => x.UserId);
        ValidateCurrencyName(x => x.CurrencyName);
    }
}
