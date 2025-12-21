using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface ITransferRepository
{
    Task AddAsync(Transfer entity, CancellationToken ct = default);
    Task UpdateAsync(Transfer entity, CancellationToken ct = default);
    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Transfer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
}

