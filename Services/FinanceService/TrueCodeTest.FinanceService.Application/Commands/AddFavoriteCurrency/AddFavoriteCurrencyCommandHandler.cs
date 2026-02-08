using MediatR;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;

public class AddFavoriteCurrencyCommandHandler : IRequestHandler<AddFavoriteCurrencyCommand, AddFavoriteCurrencyResult>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserCurrencyRepository _userCurrencyRepository;

    public AddFavoriteCurrencyCommandHandler(
        ICurrencyRepository currencyRepository,
        IUserCurrencyRepository userCurrencyRepository)
    {
        _currencyRepository = currencyRepository;
        _userCurrencyRepository = userCurrencyRepository;
    }

    public async Task<AddFavoriteCurrencyResult> Handle(AddFavoriteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _currencyRepository.GetByNameAsync(request.CurrencyName, cancellationToken);

        if (currency == null)
        {
            return new AddFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта не найдена"
            };
        }

        var exists = await _userCurrencyRepository.ExistsAsync(request.UserId, currency.Id, cancellationToken);

        if (exists)
        {
            return new AddFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта уже в избранном"
            };
        }

        await _userCurrencyRepository.AddAsync(new UserCurrency
        {
            UserId = request.UserId,
            CurrencyId = currency.Id
        }, cancellationToken);

        return new AddFavoriteCurrencyResult { Success = true };
    }
}
