using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface ITransactionLimitRepository
{
    Task<TransactionLimit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TransactionLimit>> GetAllAsync(CancellationToken ct = default);
    Task<TransactionLimit?> GetApplicableLimitAsync(LimitType type, LimitScope scope, Currency currency, Guid? scopeId = null, UserRole? role = null, CancellationToken ct = default);
    Task AddAsync(TransactionLimit limit, CancellationToken ct = default);
    Task<List<TransactionLimit>> GetActiveLimitsAsync(CancellationToken ct = default);
}
