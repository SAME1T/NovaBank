namespace NovaBank.Contracts.Admin;

public sealed record CustomerSummaryResponse(
    Guid CustomerId,
    string FullName,
    string NationalIdMasked,
    string Role);

public sealed record AccountAdminResponse(
    Guid AccountId,
    string Iban,
    string Currency,
    decimal Balance,
    decimal OverdraftLimit,
    string Status);

public sealed record UpdateOverdraftRequest(decimal OverdraftLimit);

public sealed record UpdateAccountStatusRequest(string Status);
