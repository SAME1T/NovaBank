using NovaBank.Core.Enums;
namespace NovaBank.Contracts.Cards;
public sealed record IssueCardRequest(Guid AccountId, CardType CardType, decimal? CreditLimit);
public sealed record CardResponse(Guid Id, Guid AccountId, string MaskedPan, int ExpiryMonth, int ExpiryYear, string CardType, string CardStatus, decimal? CreditLimit, decimal? AvailableLimit);
public sealed record CardBlockRequest(Guid CardId, string? Reason);
public sealed record CardUnblockRequest(Guid CardId);

