using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface ICreditCardApplicationRepository
{
    Task<CreditCardApplication?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<CreditCardApplication>> GetPendingApplicationsAsync(CancellationToken ct = default);
    Task<List<CreditCardApplication>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<bool> HasPendingApplicationAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(CreditCardApplication application, CancellationToken ct = default);
}

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Card>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<List<Card>> GetCreditCardsByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<List<Card>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Card card, CancellationToken ct = default);
}
