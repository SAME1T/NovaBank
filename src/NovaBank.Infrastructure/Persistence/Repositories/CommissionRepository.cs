using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class CommissionRepository : ICommissionRepository
{
    private readonly BankDbContext _context;

    public CommissionRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<Commission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Commissions.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<Commission>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Commissions.ToListAsync(ct);
    }

    public async Task<List<Commission>> GetActiveCommissionsAsync(CommissionType type, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Commissions
            .Where(x => x.IsActive && x.CommissionType == type && x.ValidFrom <= now && (x.ValidUntil == null || x.ValidUntil > now))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Commission commission, CancellationToken ct = default)
    {
        await _context.Commissions.AddAsync(commission, ct);
    }
}
