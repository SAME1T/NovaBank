using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Admin;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Admin;

public class AdminService : IAdminService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public AdminService(
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        CurrentUser currentUser)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CustomerSummaryResponse>>> SearchCustomersAsync(string? searchTerm, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<CustomerSummaryResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var customers = await _customerRepository.SearchAsync(searchTerm, ct);
        var responses = customers.Select(c =>
        {
            var nationalId = c.NationalId.Value;
            var masked = nationalId.Length == 11 
                ? $"{nationalId[..6]}****{nationalId[^1]}" 
                : nationalId;
            return new CustomerSummaryResponse(
                c.Id,
                $"{c.FirstName} {c.LastName}",
                masked,
                c.Role.ToString()
            );
        }).ToList();

        return Result<List<CustomerSummaryResponse>>.Success(responses);
    }

    public async Task<Result<List<AccountAdminResponse>>> GetCustomerAccountsAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<AccountAdminResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId, ct);
        var responses = accounts.Select(a => new AccountAdminResponse(
            a.Id,
            a.Iban.Value,
            a.Currency.ToString(),
            a.Balance.Amount,
            a.OverdraftLimit,
            a.Status.ToString()
        )).ToList();

        return Result<List<AccountAdminResponse>>.Success(responses);
    }

    public async Task<Result> UpdateOverdraftLimitAsync(Guid accountId, decimal overdraftLimit, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        if (overdraftLimit < 0)
            return Result.Failure(ErrorCodes.Validation, "Overdraft limit negatif olamaz.");

        await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var account = await _accountRepository.GetByIdForUpdateAsync(accountId, transactionCt);
            if (account is null)
                throw new InvalidOperationException(ErrorCodes.AccountNotFound);

            account.UpdateOverdraftLimit(overdraftLimit);
            await _accountRepository.UpdateAsync(account, transactionCt);
        }, ct);

        return Result.Success();
    }

    public async Task<Result> UpdateAccountStatusAsync(Guid accountId, AccountStatus status, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var account = await _accountRepository.GetByIdForUpdateAsync(accountId, transactionCt);
            if (account is null)
                throw new InvalidOperationException(ErrorCodes.AccountNotFound);

            switch (status)
            {
                case AccountStatus.Active:
                    account.Activate();
                    break;
                case AccountStatus.Frozen:
                    account.Freeze();
                    break;
                case AccountStatus.Closed:
                    account.Close();
                    break;
            }

            await _accountRepository.UpdateAsync(account, transactionCt);
        }, ct);

        return Result.Success();
    }
}

