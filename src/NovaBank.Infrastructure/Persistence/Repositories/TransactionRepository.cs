using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly BankDbContext _context;

    public TransactionRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction entity, CancellationToken ct = default)
    {
        await _context.Transactions.AddAsync(entity, ct);
        // SaveChanges will be handled by UnitOfWork
    }

    public async Task<List<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Transaction>> GetByAccountIdAndDateRangeAsync(Guid accountId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionDate >= from && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }
}

