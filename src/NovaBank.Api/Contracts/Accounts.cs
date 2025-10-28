using NovaBank.Core.Enums;
namespace NovaBank.Api.Contracts;
public sealed record CreateAccountRequest(Guid CustomerId, long AccountNo, Currency Currency, decimal OverdraftLimit);
public sealed record AccountResponse(Guid Id, Guid CustomerId, long AccountNo, string Iban, string Currency, decimal Balance, decimal OverdraftLimit);
