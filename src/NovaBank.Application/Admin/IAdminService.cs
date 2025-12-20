using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Admin;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Admin;

public interface IAdminService
{
    Task<Result<List<CustomerSummaryResponse>>> SearchCustomersAsync(string? searchTerm, CancellationToken ct = default);
    Task<Result<List<AccountAdminResponse>>> GetCustomerAccountsAsync(Guid customerId, CancellationToken ct = default);
    Task<Result> UpdateOverdraftLimitAsync(Guid accountId, decimal overdraftLimit, CancellationToken ct = default);
    Task<Result> UpdateAccountStatusAsync(Guid accountId, AccountStatus status, CancellationToken ct = default);
}

