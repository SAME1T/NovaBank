using NovaBank.Core.Enums;
namespace NovaBank.Api.Contracts;
public sealed record DepositRequest(Guid AccountId, decimal Amount, Currency Currency, string? Description);
public sealed record WithdrawRequest(Guid AccountId, decimal Amount, Currency Currency, string? Description);
public sealed record TransferInternalRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount, Currency Currency, string? Description);
public sealed record TransferExternalRequest(Guid FromAccountId, string ToIban, decimal Amount, Currency Currency, string? Description);
public sealed record TransactionResponse(Guid Id, Guid AccountId, decimal Amount, string Currency, string Direction, string? Description, string ReferenceCode, DateTime CreatedAt);
