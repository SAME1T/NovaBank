using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly BankDbContext _context;

    public TransferRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transfer entity, CancellationToken ct = default)
    {
        await _context.Transfers.AddAsync(entity, ct);
        // SaveChanges will be handled by UnitOfWork
    }

    public Task UpdateAsync(Transfer entity, CancellationToken ct = default)
    {
        _context.Transfers.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Transfers.FindAsync(new object[] { id }, ct);
    }

    public async Task<Transfer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // PostgreSQL FOR UPDATE ile satır kilitleme
        return await _context.Transfers
            .FromSqlInterpolated($"SELECT * FROM bank_transfers WHERE \"Id\" = {id} FOR UPDATE")
            .FirstOrDefaultAsync(ct);
    }
}

