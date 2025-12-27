using NovaBank.Contracts.Limits;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Limits;

public interface ILimitService
{
    Task<TransactionLimitResponse> CreateLimitAsync(CreateLimitRequest request, CancellationToken ct = default);
    Task<List<TransactionLimitResponse>> GetActiveLimitsAsync(CancellationToken ct = default);
    Task UpdateLimitAsync(Guid id, UpdateLimitRequest request, CancellationToken ct = default);
    Task DeactivateLimitAsync(Guid id, CancellationToken ct = default);
    
    // Check methods
    Task<(bool IsAllowed, string? Error)> CheckLimitAsync(LimitType type, LimitScope scope, Currency currency, decimal amount, Guid? scopeId = null, UserRole? role = null, CancellationToken ct = default);
    Task<bool> RequiresApprovalAsync(LimitType type, LimitScope scope, Currency currency, decimal amount, Guid? scopeId = null, UserRole? role = null, CancellationToken ct = default);
}
