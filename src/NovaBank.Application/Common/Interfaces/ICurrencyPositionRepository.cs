using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface ICurrencyPositionRepository
{
    Task<CurrencyPosition?> GetByCustomerAndCurrencyAsync(Guid customerId, Currency currency, CancellationToken ct = default);
    Task<List<CurrencyPosition>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(CurrencyPosition position, CancellationToken ct = default);
    Task UpdateAsync(CurrencyPosition position, CancellationToken ct = default);
}
