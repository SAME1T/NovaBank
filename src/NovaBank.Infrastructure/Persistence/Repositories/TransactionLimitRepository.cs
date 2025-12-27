using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class TransactionLimitRepository : ITransactionLimitRepository
{
    private readonly BankDbContext _context;

    public TransactionLimitRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionLimit?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TransactionLimits.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<TransactionLimit>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.TransactionLimits.ToListAsync(ct);
    }

    public async Task<TransactionLimit?> GetApplicableLimitAsync(LimitType type, LimitScope scope, Currency currency, Guid? scopeId = null, UserRole? role = null, CancellationToken ct = default)
    {
        var query = _context.TransactionLimits
            .Where(x => x.IsActive && x.LimitType == type && x.Currency == currency && x.Scope == scope);

        if (scope == LimitScope.Customer || scope == LimitScope.Account)
        {
            query = query.Where(x => x.ScopeId == scopeId);
        }
        else if (scope == LimitScope.Role)
        {
            query = query.Where(x => x.ScopeRole == role);
        }
        else if (scope == LimitScope.Global)
        {
            // No extra filter
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(TransactionLimit limit, CancellationToken ct = default)
    {
        await _context.TransactionLimits.AddAsync(limit, ct);
    }

    public async Task<List<TransactionLimit>> GetActiveLimitsAsync(CancellationToken ct = default)
    {
        return await _context.TransactionLimits.Where(x => x.IsActive).ToListAsync(ct);
    }
}
