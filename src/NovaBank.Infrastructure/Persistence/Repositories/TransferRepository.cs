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

    public async Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Transfers.FindAsync(new object[] { id }, ct);
    }
}

