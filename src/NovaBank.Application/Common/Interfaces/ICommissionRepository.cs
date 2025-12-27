using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface ICommissionRepository
{
    Task<Commission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Commission>> GetAllAsync(CancellationToken ct = default);
    Task<List<Commission>> GetActiveCommissionsAsync(CommissionType type, CancellationToken ct = default);
    Task AddAsync(Commission commission, CancellationToken ct = default);
}
