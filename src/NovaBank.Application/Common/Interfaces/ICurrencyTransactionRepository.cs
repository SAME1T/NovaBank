using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface ICurrencyTransactionRepository
{
    Task<List<CurrencyTransaction>> GetByCustomerIdAsync(Guid customerId, int take = 50, CancellationToken ct = default);
    Task<List<CurrencyTransaction>> GetByCustomerAndCurrencyAsync(Guid customerId, Currency currency, int take = 50, CancellationToken ct = default);
    Task AddAsync(CurrencyTransaction transaction, CancellationToken ct = default);
}
