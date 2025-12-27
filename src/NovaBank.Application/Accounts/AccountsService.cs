using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Accounts;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Services;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.Accounts;

public class AccountsService : IAccountsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IIbanGenerator _ibanGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AccountsService(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IIbanGenerator ibanGenerator,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _ibanGenerator = ibanGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result<AccountResponse>.Failure(ErrorCodes.NotFound, "Customer bulunamadı.");

        if (await _accountRepository.ExistsByAccountNoAsync(request.AccountNo, ct))
            return Result<AccountResponse>.Failure(ErrorCodes.Conflict, "AccountNo mevcut.");

        // Otomatik IBAN oluştur
        string generatedIban;
        do
        {
            generatedIban = _ibanGenerator.GenerateIban();
        } while (await _accountRepository.ExistsByIbanAsync(generatedIban, ct));

        var account = new Account(
            request.CustomerId,
            new AccountNo(request.AccountNo),
            new Iban(generatedIban),
            request.Currency,
            new Money(0m, request.Currency),
            Math.Max(0, request.OverdraftLimit)
        );

        await _accountRepository.AddAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct); // Veritabanına kaydet!

        var response = new AccountResponse(
            account.Id,
            account.CustomerId,
            account.AccountNo.Value,
            account.Iban.Value,
            account.Currency.ToString(),
            account.Balance.Amount,
            account.OverdraftLimit
        );

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account is null)
            return Result<AccountResponse>.Failure(ErrorCodes.NotFound, "Account bulunamadı.");

        var response = new AccountResponse(
            account.Id,
            account.CustomerId,
            account.AccountNo.Value,
            account.Iban.Value,
            account.Currency.ToString(),
            account.Balance.Amount,
            account.OverdraftLimit
        );

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result<List<AccountResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId, ct);
        var responses = accounts.Select(a => new AccountResponse(
            a.Id,
            a.CustomerId,
            a.AccountNo.Value,
            a.Iban.Value,
            a.Currency.ToString(),
            a.Balance.Amount,
            a.OverdraftLimit
        )).ToList();

        return Result<List<AccountResponse>>.Success(responses);
    }

    public async Task<Result<AccountResponse>> GetByAccountNoAsync(long accountNo, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByAccountNoAsync(accountNo, ct);
        if (account is null)
            return Result<AccountResponse>.Failure(ErrorCodes.NotFound, "Account bulunamadı.");

        var response = new AccountResponse(
            account.Id,
            account.CustomerId,
            account.AccountNo.Value,
            account.Iban.Value,
            account.Currency.ToString(),
            account.Balance.Amount,
            account.OverdraftLimit
        );

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result<AccountResponse>> GetByIbanAsync(string iban, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIbanAsync(iban, ct);
        if (account is null)
            return Result<AccountResponse>.Failure(ErrorCodes.NotFound, "Account bulunamadı.");

        var response = new AccountResponse(
            account.Id,
            account.CustomerId,
            account.AccountNo.Value,
            account.Iban.Value,
            account.Currency.ToString(),
            account.Balance.Amount,
            account.OverdraftLimit
        );

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result<string>> GetOwnerNameByIbanAsync(string iban, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIbanAsync(iban, ct);
        if (account is null)
            return Result<string>.Failure(ErrorCodes.NotFound, "Account bulunamadı.");

        var customer = await _customerRepository.GetByIdAsync(account.CustomerId, ct);
        if (customer is null)
            return Result<string>.Failure(ErrorCodes.NotFound, "Customer bulunamadı.");

        var fullName = $"{customer.FirstName} {customer.LastName}";
        return Result<string>.Success(fullName);
    }

    public async Task<Result<List<AccountResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var accounts = await _accountRepository.GetAllAsync(ct);
        var responses = accounts.Select(a => new AccountResponse(
            a.Id,
            a.CustomerId,
            a.AccountNo.Value,
            a.Iban.Value,
            a.Currency.ToString(),
            a.Balance.Amount,
            a.OverdraftLimit
        )).ToList();
        return Result<List<AccountResponse>>.Success(responses);
    }
}

