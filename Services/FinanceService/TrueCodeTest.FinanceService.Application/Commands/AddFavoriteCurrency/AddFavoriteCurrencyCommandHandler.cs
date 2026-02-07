using MediatR;
using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;

public class AddFavoriteCurrencyCommandHandler : IRequestHandler<AddFavoriteCurrencyCommand, AddFavoriteCurrencyResult>
{
    private readonly ApplicationDbContext _dbContext;

    public AddFavoriteCurrencyCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AddFavoriteCurrencyResult> Handle(AddFavoriteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Name == request.CurrencyName, cancellationToken);

        if (currency == null)
        {
            return new AddFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта не найдена"
            };
        }

        var existing = await _dbContext.UserCurrencies
            .FirstOrDefaultAsync(uc => uc.UserId == request.UserId && uc.CurrencyId == currency.Id, cancellationToken);

        if (existing != null)
        {
            return new AddFavoriteCurrencyResult
            {
                Success = false,
                ErrorMessage = "Валюта уже в избранном"
            };
        }

        _dbContext.UserCurrencies.Add(new UserCurrency
        {
            UserId = request.UserId,
            CurrencyId = currency.Id
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddFavoriteCurrencyResult { Success = true };
    }
}
