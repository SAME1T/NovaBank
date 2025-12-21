using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetRepository : IPasswordResetRepository
{
    private readonly BankDbContext _db;

    public PasswordResetRepository(BankDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        await _db.PasswordResetTokens.AddAsync(token, ct);
    }

    public async Task<PasswordResetToken?> GetLatestValidAsync(Guid customerId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _db.PasswordResetTokens
            .Where(t => t.CustomerId == customerId
                && !t.IsUsed
                && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        _db.PasswordResetTokens.Update(token);
        return Task.CompletedTask;
    }
}

