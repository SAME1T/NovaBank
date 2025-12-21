using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByTcknAsync(string tckn, CancellationToken ct = default);
    Task<List<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<List<Customer>> SearchAsync(string? searchTerm, CancellationToken ct = default);
    Task<bool> ExistsByTcknAsync(string tckn, CancellationToken ct = default);
    Task AddAsync(Customer entity, CancellationToken ct = default);
    Task UpdateAsync(Customer entity, CancellationToken ct = default);
    Task<Customer?> FindByEmailOrNationalIdAsync(string emailOrNationalId, CancellationToken ct = default);
}

