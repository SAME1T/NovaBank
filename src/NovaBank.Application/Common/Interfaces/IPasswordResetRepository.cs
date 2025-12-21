using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface IPasswordResetRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken ct = default);
    Task<PasswordResetToken?> GetLatestValidAsync(Guid customerId, CancellationToken ct = default);
    Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default);
}

