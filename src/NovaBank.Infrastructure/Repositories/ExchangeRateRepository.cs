using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly BankDbContext _context;

    public ExchangeRateRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetLatestAsync(Currency baseCurrency, Currency targetCurrency, CancellationToken ct = default)
    {
        return await _context.ExchangeRates
            .Where(r => r.BaseCurrency == baseCurrency && r.TargetCurrency == targetCurrency)
            .OrderByDescending(r => r.EffectiveDate)
            .ThenByDescending(r => r.UpdatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ExchangeRate>> GetAllLatestAsync(CancellationToken ct = default)
    {
        // Her döviz çifti için son kuru getir
        var rates = await _context.ExchangeRates
            .GroupBy(r => new { r.BaseCurrency, r.TargetCurrency })
            .Select(g => g.OrderByDescending(r => r.EffectiveDate).ThenByDescending(r => r.UpdatedAt).First())
            .ToListAsync(ct);
            
        return rates;
    }

    public async Task AddOrUpdateAsync(ExchangeRate rate, CancellationToken ct = default)
    {
        // EffectiveDate'i UTC olarak karşılaştır
        var effectiveDateUtc = DateTime.SpecifyKind(rate.EffectiveDate.Date, DateTimeKind.Utc);
        
        var existing = await _context.ExchangeRates
            .FirstOrDefaultAsync(r => 
                r.BaseCurrency == rate.BaseCurrency && 
                r.TargetCurrency == rate.TargetCurrency &&
                r.EffectiveDate.Date == effectiveDateUtc.Date, ct);

        if (existing != null)
        {
            existing.Update(rate.BuyRate, rate.SellRate);
        }
        else
        {
            await _context.ExchangeRates.AddAsync(rate, ct);
        }
    }
}
