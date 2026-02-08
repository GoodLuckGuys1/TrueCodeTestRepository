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

    public async Task<Currency?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<Currency?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Currency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Currencies
            .ToListAsync(cancellationToken);
    }

    public async Task<Currency> CreateAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync(cancellationToken);
        return currency;
    }

    public async Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var currency = await GetByIdAsync(id, cancellationToken);
        if (currency != null)
        {
            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync(cancellationToken);
        }
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
