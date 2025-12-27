using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface IBillInstitutionRepository
{
    Task<BillInstitution?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BillInstitution>> GetActiveAsync(CancellationToken ct = default);
    Task<List<BillInstitution>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(BillInstitution institution, CancellationToken ct = default);
    Task UpdateAsync(BillInstitution institution, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IBillPaymentRepository
{
    Task<BillPayment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BillPayment>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<List<BillPayment>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(BillPayment payment, CancellationToken ct = default);
}
