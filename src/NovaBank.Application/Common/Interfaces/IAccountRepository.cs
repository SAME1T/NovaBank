using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByIbanAsync(string iban, CancellationToken ct = default);
    Task<Account?> GetByAccountNoAsync(long accountNo, CancellationToken ct = default);
    Task<List<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<bool> ExistsByAccountNoAsync(long accountNo, CancellationToken ct = default);
    Task<bool> ExistsByIbanAsync(string iban, CancellationToken ct = default);
    Task AddAsync(Account entity, CancellationToken ct = default);
    Task UpdateAsync(Account entity, CancellationToken ct = default);
    Task<List<Account>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an account by ID with FOR UPDATE lock for concurrent transaction safety.
    /// </summary>
    Task<Account?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets an account by IBAN with FOR UPDATE lock for concurrent transaction safety.
    /// </summary>
    Task<Account?> GetByIbanForUpdateAsync(string iban, CancellationToken ct = default);
}

