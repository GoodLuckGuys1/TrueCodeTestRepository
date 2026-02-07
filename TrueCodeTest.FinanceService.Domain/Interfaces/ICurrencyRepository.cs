using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetUserFavoriteCurrenciesAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default);
}
