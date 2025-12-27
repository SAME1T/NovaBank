namespace NovaBank.Contracts.Admin;

public sealed record CustomerSummaryResponse(
    Guid CustomerId,
    string FullName,
    string NationalIdMasked,
    string Role,
    bool IsActive,
    bool IsApproved);

public sealed record AccountAdminResponse(
    Guid AccountId,
    string Iban,
    string Currency,
    decimal Balance,
    decimal OverdraftLimit,
    string Status);

public sealed record UpdateOverdraftRequest(decimal OverdraftLimit);

public sealed record UpdateAccountStatusRequest(string Status);

public sealed record UpdateCustomerActiveRequest(bool IsActive);

public sealed record UpdateCustomerActiveResponse(Guid CustomerId, bool IsActive);

public sealed record ResetCustomerPasswordResponse(
    Guid CustomerId,
    string TemporaryPassword);

public sealed record AuditLogResponse(
    Guid Id,
    DateTime CreatedAt,
    Guid? ActorCustomerId,
    string ActorRole,
    string Action,
    string? EntityType,
    string? EntityId,
    bool Success,
    string? ErrorCode,
    string? Summary);

public sealed record AuditLogQuery(
    DateTime? From,
    DateTime? To,
    string? Search,
    string? Action,
    bool? Success,
    int Take = 200);

public sealed record PendingApprovalResponse(
    Guid CustomerId,
    string FullName,
    string NationalId,
    string Email,
    DateTime CreatedAt);

public sealed record ApproveCustomerResponse(
    Guid CustomerId,
    bool IsApproved);
