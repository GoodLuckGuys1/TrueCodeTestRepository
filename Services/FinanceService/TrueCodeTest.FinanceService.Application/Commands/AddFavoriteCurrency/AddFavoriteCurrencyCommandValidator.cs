using TrueCodeTest.Shared.Domain.Validators;

namespace TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;

public class AddFavoriteCurrencyCommandValidator : BaseValidator<AddFavoriteCurrencyCommand>
{
    public AddFavoriteCurrencyCommandValidator()
    {
        ValidateUserId(x => x.UserId);
        ValidateCurrencyName(x => x.CurrencyName);
    }
}
