using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Transactions;

namespace NovaBank.Application.Transactions;

public interface ITransactionsService
{
    Task<Result<TransactionResponse>> DepositAsync(DepositRequest request, CancellationToken ct = default);
    Task<Result<TransactionResponse>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
}

