using Microsoft.EntityFrameworkCore;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Infrastructure.Repositories;

public class UserCurrencyRepository : IUserCurrencyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserCurrencyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserCurrency?> GetByUserAndCurrencyAsync(int userId, int currencyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserCurrencies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CurrencyId == currencyId, cancellationToken);
    }
    

    public async Task<UserCurrency> AddAsync(UserCurrency userCurrency, CancellationToken cancellationToken = default)
    {
        _dbContext.UserCurrencies.Add(userCurrency);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return userCurrency;
    }

    public async Task RemoveAsync(UserCurrency userCurrency, CancellationToken cancellationToken = default)
    {
        _dbContext.UserCurrencies.Remove(userCurrency);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int userId, int currencyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserCurrencies
            .AnyAsync(uc => uc.UserId == userId && uc.CurrencyId == currencyId, cancellationToken);
    }
}
