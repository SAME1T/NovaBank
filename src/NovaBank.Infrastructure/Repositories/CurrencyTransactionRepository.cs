using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Repositories;

public class CurrencyTransactionRepository : ICurrencyTransactionRepository
{
    private readonly BankDbContext _context;

    public CurrencyTransactionRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<List<CurrencyTransaction>> GetByCustomerIdAsync(Guid customerId, int take = 50, CancellationToken ct = default)
    {
        return await _context.CurrencyTransactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<List<CurrencyTransaction>> GetByCustomerAndCurrencyAsync(Guid customerId, Currency currency, int take = 50, CancellationToken ct = default)
    {
        return await _context.CurrencyTransactions
            .Where(t => t.CustomerId == customerId && t.Currency == currency)
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CurrencyTransaction transaction, CancellationToken ct = default)
    {
        await _context.CurrencyTransactions.AddAsync(transaction, ct);
    }
}
