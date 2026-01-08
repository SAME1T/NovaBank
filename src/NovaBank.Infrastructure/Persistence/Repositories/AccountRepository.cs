using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankDbContext _context;

    public AccountRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Accounts.FindAsync(new object[] { id }, ct);
    }

    public async Task<Account?> GetByIbanAsync(string iban, CancellationToken ct = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Iban == new Iban(iban), ct);
    }

    public async Task<Account?> GetByAccountNoAsync(long accountNo, CancellationToken ct = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNo == new AccountNo(accountNo), ct);
    }

    public async Task<List<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByAccountNoAsync(long accountNo, CancellationToken ct = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.AccountNo == new AccountNo(accountNo), ct);
    }

    public async Task<bool> ExistsByIbanAsync(string iban, CancellationToken ct = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Iban == new Iban(iban), ct);
    }

    public async Task AddAsync(Account entity, CancellationToken ct = default)
    {
        await _context.Accounts.AddAsync(entity, ct);
        // SaveChanges will be handled by UnitOfWork
    }

    public async Task UpdateAsync(Account entity, CancellationToken ct = default)
    {
        _context.Accounts.Update(entity);
        // SaveChanges will be handled by UnitOfWork
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Accounts.ToListAsync(ct);
    }

    public async Task<Account?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // PostgreSQL FOR UPDATE ile satır kilitleme
        return await _context.Accounts
            .FromSqlInterpolated($"SELECT * FROM bank_accounts WHERE \"Id\" = {id} FOR UPDATE")
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Account?> GetByIbanForUpdateAsync(string iban, CancellationToken ct = default)
    {
        // PostgreSQL FOR UPDATE ile satır kilitleme
        // IBAN converter kullanarak değeri alıyoruz
        var ibanValue = new Iban(iban).Value;
        return await _context.Accounts
            .FromSqlInterpolated($"SELECT * FROM bank_accounts WHERE \"iban\" = {ibanValue} FOR UPDATE")
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Account>> GetPendingApprovalsAsync(CancellationToken ct = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Status == AccountStatus.PendingApproval)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Accounts.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _context.Accounts.Remove(entity);
            // SaveChanges will be handled by UnitOfWork
        }
    }
}

