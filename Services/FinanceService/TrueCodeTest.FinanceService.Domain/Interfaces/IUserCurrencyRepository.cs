using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Domain.Interfaces;

public interface IUserCurrencyRepository
{
    Task<UserCurrency?> GetByUserAndCurrencyAsync(int userId, int currencyId,
        CancellationToken cancellationToken = default);

    Task<UserCurrency> AddAsync(UserCurrency userCurrency, CancellationToken cancellationToken = default);
    Task RemoveAsync(UserCurrency userCurrency, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int userId, int currencyId, CancellationToken cancellationToken = default);
}