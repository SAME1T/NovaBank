using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public sealed class CreditCardApplicationRepository : ICreditCardApplicationRepository
{
    private readonly BankDbContext _context;

    public CreditCardApplicationRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<CreditCardApplication?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CreditCardApplications.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<CreditCardApplication>> GetPendingApplicationsAsync(CancellationToken ct = default)
    {
        return await _context.CreditCardApplications
            .Where(x => x.Status == ApplicationStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<CreditCardApplication>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.CreditCardApplications
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> HasPendingApplicationAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.CreditCardApplications
            .AnyAsync(x => x.CustomerId == customerId && x.Status == ApplicationStatus.Pending, ct);
    }

    public async Task AddAsync(CreditCardApplication application, CancellationToken ct = default)
    {
        await _context.CreditCardApplications.AddAsync(application, ct);
    }
}

public sealed class CardRepository : ICardRepository
{
    private readonly BankDbContext _context;

    public CardRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Cards.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<Card>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Cards
            .Where(x => x.CustomerId == customerId)
            .ToListAsync(ct);
    }

    public async Task<List<Card>> GetCreditCardsByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Cards
            .Where(x => x.CustomerId == customerId && x.CardType == CardType.Credit && x.IsApproved)
            .ToListAsync(ct);
    }

    public async Task<List<Card>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Cards.ToListAsync(ct);
    }

    public async Task AddAsync(Card card, CancellationToken ct = default)
    {
        await _context.Cards.AddAsync(card, ct);
    }
}
