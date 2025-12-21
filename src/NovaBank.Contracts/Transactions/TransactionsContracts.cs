using NovaBank.Core.Enums;
namespace NovaBank.Contracts.Transactions;
public sealed record DepositRequest(Guid AccountId, decimal Amount, Currency Currency, string? Description);
public sealed record WithdrawRequest(Guid AccountId, decimal Amount, Currency Currency, string? Description);
public sealed record TransferInternalRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount, Currency Currency, string? Description);
public sealed record TransferExternalRequest(Guid FromAccountId, string ToIban, decimal Amount, Currency Currency, string? Description);
public sealed record TransactionResponse(Guid Id, Guid AccountId, decimal Amount, string Currency, string Direction, string? Description, string ReferenceCode, DateTime CreatedAt);
public sealed record TransferResponse(Guid Id, Guid FromAccountId, Guid ToAccountId, decimal Amount, string Currency, string Channel, string Status, DateTime CreatedAt);
public sealed record ReverseTransferRequest(Guid TransferId, string? Reason);
public sealed record ReverseTransferResponse(Guid OriginalTransferId, Guid ReversalTransferId, DateTime ReversedAt);

