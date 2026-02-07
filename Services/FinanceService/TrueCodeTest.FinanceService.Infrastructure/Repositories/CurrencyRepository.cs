using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ApplicationDbContext _context;

    public CurrencyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Currency>> GetUserFavoriteCurrenciesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserCurrencies
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Currency)
            .Select(uc => uc.Currency)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Currencies.ToListAsync(cancellationToken);
    }
}
