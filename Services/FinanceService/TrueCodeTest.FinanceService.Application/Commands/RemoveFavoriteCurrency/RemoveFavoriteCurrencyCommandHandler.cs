using MediatR;
using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;

public class RemoveFavoriteCurrencyCommandHandler : IRequestHandler<RemoveFavoriteCurrencyCommand, RemoveFavoriteCurrencyResult>
{
    private readonly ApplicationDbContext _dbContext;

    public RemoveFavoriteCurrencyCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RemoveFavoriteCurrencyResult> Handle(RemoveFavoriteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Name == request.CurrencyName, cancellationToken);

        if (currency == null)
        {
            return new RemoveFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта не найдена"
            };
        }

        var userCurrency = await _dbContext.UserCurrencies
            .FirstOrDefaultAsync(uc => uc.UserId == request.UserId && uc.CurrencyId == currency.Id, cancellationToken);

        if (userCurrency == null)
        {
            return new RemoveFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта не в избранном"
            };
        }

        _dbContext.UserCurrencies.Remove(userCurrency);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RemoveFavoriteCurrencyResult { Success = true };
    }
}
