using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class KycRepository : IKycRepository
{
    private readonly BankDbContext _context;

    public KycRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<KycVerification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.KycVerifications.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<KycVerification>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.KycVerifications
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<KycVerification>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _context.KycVerifications
            .Where(x => x.Status == VerificationStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(KycVerification verification, CancellationToken ct = default)
    {
        await _context.KycVerifications.AddAsync(verification, ct);
    }
}
