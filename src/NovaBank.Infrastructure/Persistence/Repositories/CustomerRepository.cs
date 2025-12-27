using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankDbContext _context;

    public CustomerRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Customers.FindAsync(new object[] { id }, ct);
    }

    public async Task<Customer?> GetByTcknAsync(string tckn, CancellationToken ct = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.NationalId == new NationalId(tckn), ct);
    }

    public async Task<List<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Customers.ToListAsync(ct);
    }

    public async Task<List<Customer>> SearchAsync(string? searchTerm, CancellationToken ct = default)
    {
        var term = (searchTerm ?? "").Trim();
        if (term.Length == 0)
        {
            // Son 50 müşteri dön
            return await _context.Customers
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync(ct);
        }

        var pattern = $"%{term}%";
        return await _context.Customers
            .AsNoTracking()
            .Where(c =>
                EF.Functions.ILike(c.FirstName, pattern) ||
                EF.Functions.ILike(c.LastName, pattern) ||
                EF.Functions.ILike(EF.Property<string>(c, "national_id"), pattern) ||
                (c.Email != null && EF.Functions.ILike(c.Email, pattern))
            )
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<List<Customer>> GetPendingApprovalsAsync(CancellationToken ct = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(c => !c.IsApproved && c.Role == NovaBank.Core.Enums.UserRole.Customer)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Customer?> FindByEmailOrNationalIdAsync(string emailOrNationalId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(emailOrNationalId))
            return null;

        var input = emailOrNationalId.Trim();

        // Email kontrolü (@ işareti varsa email kabul et)
        if (input.Contains('@'))
        {
            // Email için case-insensitive eşitlik kontrolü (EF translate hatası olmaması için SQL)
            return await _context.Customers
                .FromSqlInterpolated($@"SELECT * FROM bank_customers WHERE ""Email"" IS NOT NULL AND ""Email"" ILIKE {input} LIMIT 1")
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
        }

        // TCKN (NationalId) ile arama - direkt eşitlik (EF translate hatası olmaması için SQL)
        return await _context.Customers
            .FromSqlInterpolated($@"SELECT * FROM bank_customers WHERE national_id = {input} LIMIT 1")
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsByTcknAsync(string tckn, CancellationToken ct = default)
    {
        return await _context.Customers
            .AnyAsync(c => c.NationalId == new NationalId(tckn), ct);
    }

    public async Task AddAsync(Customer entity, CancellationToken ct = default)
    {
        await _context.Customers.AddAsync(entity, ct);
        // SaveChanges will be handled by UnitOfWork
    }

    public async Task UpdateAsync(Customer entity, CancellationToken ct = default)
    {
        _context.Customers.Update(entity);
        // SaveChanges will be handled by UnitOfWork
    }
}

