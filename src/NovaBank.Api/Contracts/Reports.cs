namespace NovaBank.Api.Contracts;
public sealed record AccountStatementItem(DateTime CreatedAt, string Direction, decimal Amount, string Currency, string? Description, string ReferenceCode);
public sealed record AccountStatementResponse(Guid AccountId, DateTime From, DateTime To, decimal OpeningBalance, decimal TotalCredit, decimal TotalDebit, decimal ClosingBalance, IReadOnlyList<AccountStatementItem> Items);
public sealed record CustomerSummaryResponse(Guid CustomerId, string FullName, int AccountCount, decimal TotalBalanceTry, int CardCount, int LoanCount);
