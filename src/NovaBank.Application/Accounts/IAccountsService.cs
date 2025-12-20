using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Accounts;

namespace NovaBank.Application.Accounts;

public interface IAccountsService
{
    Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request, CancellationToken ct = default);
    Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<AccountResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<Result<AccountResponse>> GetByAccountNoAsync(long accountNo, CancellationToken ct = default);
    Task<Result<AccountResponse>> GetByIbanAsync(string iban, CancellationToken ct = default);
    Task<Result<string>> GetOwnerNameByIbanAsync(string iban, CancellationToken ct = default);
}

