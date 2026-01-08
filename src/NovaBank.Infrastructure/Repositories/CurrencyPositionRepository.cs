using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Repositories;

public class CurrencyPositionRepository : ICurrencyPositionRepository
{
    private readonly BankDbContext _context;

    public CurrencyPositionRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyPosition?> GetByCustomerAndCurrencyAsync(Guid customerId, Currency currency, CancellationToken ct = default)
    {
        return await _context.CurrencyPositions
            .FirstOrDefaultAsync(p => p.CustomerId == customerId && p.Currency == currency, ct);
    }

    public async Task<List<CurrencyPosition>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.CurrencyPositions
            .Where(p => p.CustomerId == customerId)
            .OrderBy(p => p.Currency)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CurrencyPosition position, CancellationToken ct = default)
    {
        await _context.CurrencyPositions.AddAsync(position, ct);
    }

    public Task UpdateAsync(CurrencyPosition position, CancellationToken ct = default)
    {
        _context.CurrencyPositions.Update(position);
        return Task.CompletedTask;
    }
}
