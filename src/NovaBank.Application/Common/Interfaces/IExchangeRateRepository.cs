using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetLatestAsync(Currency baseCurrency, Currency targetCurrency, CancellationToken ct = default);
    Task<List<ExchangeRate>> GetAllLatestAsync(CancellationToken ct = default);
    Task AddOrUpdateAsync(ExchangeRate rate, CancellationToken ct = default);
}
