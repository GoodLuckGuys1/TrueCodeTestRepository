using MediatR;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;

public class RemoveFavoriteCurrencyCommandHandler : IRequestHandler<RemoveFavoriteCurrencyCommand, RemoveFavoriteCurrencyResult>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserCurrencyRepository _userCurrencyRepository;

    public RemoveFavoriteCurrencyCommandHandler(
        ICurrencyRepository currencyRepository,
        IUserCurrencyRepository userCurrencyRepository)
    {
        _currencyRepository = currencyRepository;
        _userCurrencyRepository = userCurrencyRepository;
    }

    public async Task<RemoveFavoriteCurrencyResult> Handle(RemoveFavoriteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _currencyRepository.GetByNameAsync(request.CurrencyName, cancellationToken);

        if (currency == null)
        {
            return new RemoveFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта не найдена"
            };
        }

        var userCurrency = await _userCurrencyRepository.GetByUserAndCurrencyAsync(request.UserId, currency.Id, cancellationToken);

        if (userCurrency == null)
        {
            return new RemoveFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта не в избранном"
            };
        }

        await _userCurrencyRepository.RemoveAsync(userCurrency, cancellationToken);

        return new RemoveFavoriteCurrencyResult { Success = true };
    }
}
