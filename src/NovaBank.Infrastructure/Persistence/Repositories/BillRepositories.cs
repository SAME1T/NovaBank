using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class BillInstitutionRepository : IBillInstitutionRepository
{
    private readonly BankDbContext _context;

    public BillInstitutionRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<BillInstitution?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BillInstitutions.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<BillInstitution>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.BillInstitutions
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<List<BillInstitution>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.BillInstitutions
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(BillInstitution institution, CancellationToken ct = default)
    {
        await _context.BillInstitutions.AddAsync(institution, ct);
    }

    public async Task UpdateAsync(BillInstitution institution, CancellationToken ct = default)
    {
        _context.BillInstitutions.Update(institution);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var inst = await _context.BillInstitutions.FindAsync(new object[] { id }, ct);
        if (inst != null)
        {
            _context.BillInstitutions.Remove(inst);
        }
    }
}

public class BillPaymentRepository : IBillPaymentRepository
{
    private readonly BankDbContext _context;

    public BillPaymentRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<BillPayment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BillPayments.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<BillPayment>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await _context.BillPayments
            .Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.PaidAt)
            .ToListAsync(ct);
    }

    public async Task<List<BillPayment>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        // Join with accounts or cards to filter by customerId
        var accountPayments = _context.BillPayments
            .Join(_context.Accounts, p => p.AccountId, a => a.Id, (p, a) => new { p, a })
            .Where(x => x.a.CustomerId == customerId)
            .Select(x => x.p);

        var cardPayments = _context.BillPayments
            .Join(_context.Cards, p => p.CardId, c => c.Id, (p, c) => new { p, c })
            .Where(x => x.c.CustomerId == customerId)
            .Select(x => x.p);

        return await accountPayments.Union(cardPayments)
            .OrderByDescending(x => x.PaidAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(BillPayment payment, CancellationToken ct = default)
    {
        await _context.BillPayments.AddAsync(payment, ct);
    }
}
