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
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync(ct);

        var term = searchTerm.Trim().ToLower();
        return await _context.Customers
            .Where(c => 
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.NationalId.Value.Contains(term) ||
                (c.Email != null && c.Email.ToLower().Contains(term)))
            .ToListAsync(ct);
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

