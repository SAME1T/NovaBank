using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Reports;

namespace NovaBank.Application.Reports;

public class ReportsService : IReportsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITransactionRepository _transactionRepository;

    public ReportsService(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<AccountStatementResponse>> GetAccountStatementAsync(Guid accountId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        if (from > to)
            return Result<AccountStatementResponse>.Failure(ErrorCodes.Validation, "'from' > 'to'");

        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account is null)
            return Result<AccountStatementResponse>.Failure(ErrorCodes.NotFound, "Account bulunamadı.");

        var transactions = await _transactionRepository.GetByAccountIdAndDateRangeAsync(accountId, from, to, ct);

        decimal credit = transactions.Where(t => t.Direction.ToString() == "Credit").Sum(t => t.Amount.Amount);
        decimal debit = transactions.Where(t => t.Direction.ToString() == "Debit").Sum(t => t.Amount.Amount);
        decimal closing = account.Balance.Amount;
        decimal opening = closing - (credit - debit);

        var items = transactions.Select(t => new AccountStatementItem(
            t.TransactionDate,
            t.Direction.ToString(),
            t.Amount.Amount,
            t.Amount.Currency.ToString(),
            t.Description,
            t.ReferenceCode
        )).ToList();

        var response = new AccountStatementResponse(
            accountId,
            from,
            to,
            opening,
            credit,
            debit,
            closing,
            items
        );

        return Result<AccountStatementResponse>.Success(response);
    }

    public async Task<Result<CustomerSummaryResponse>> GetCustomerSummaryAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer is null)
            return Result<CustomerSummaryResponse>.Failure(ErrorCodes.NotFound, "Customer bulunamadı.");

        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId, ct);
        var totalTry = accounts.Sum(a => a.Balance.Amount);

        // Cards ve Loans için repository'ye metod eklenebilir, şimdilik 0 döndürüyoruz
        var response = new CustomerSummaryResponse(
            customerId,
            $"{customer.FirstName} {customer.LastName}",
            accounts.Count,
            totalTry,
            0, // cards
            0  // loans
        );

        return Result<CustomerSummaryResponse>.Success(response);
    }
}

