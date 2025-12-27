using NovaBank.Contracts.Commissions;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Commissions;

public interface ICommissionService
{
    Task<CommissionResponse> CreateCommissionAsync(CreateCommissionRequest request, CancellationToken ct = default);
    Task<List<CommissionResponse>> GetActiveCommissionsAsync(CommissionType type, CancellationToken ct = default);
    Task UpdateCommissionAsync(Guid id, UpdateCommissionRequest request, CancellationToken ct = default);
    
    // Calculation
    Task<decimal> CalculateCommissionAsync(CommissionType type, Currency currency, decimal amount, CancellationToken ct = default);
}
