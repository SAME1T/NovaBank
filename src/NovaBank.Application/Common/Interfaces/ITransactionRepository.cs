using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction entity, CancellationToken ct = default);
    Task<List<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<List<Transaction>> GetByAccountIdAndDateRangeAsync(Guid accountId, DateTime from, DateTime to, CancellationToken ct = default);
}

