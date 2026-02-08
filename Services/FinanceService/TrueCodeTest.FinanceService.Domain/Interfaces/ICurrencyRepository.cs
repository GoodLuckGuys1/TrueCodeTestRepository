using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<Currency?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Currency?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Currency>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Currency> CreateAsync(Currency currency, CancellationToken cancellationToken = default);
    Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Currency>> GetUserFavoriteCurrenciesAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default);
}
