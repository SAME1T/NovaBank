using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface IKycRepository
{
    Task<KycVerification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<KycVerification>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<List<KycVerification>> GetPendingAsync(CancellationToken ct = default);
    Task AddAsync(KycVerification verification, CancellationToken ct = default);
}
