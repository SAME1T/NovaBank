using NovaBank.Core.Enums;

namespace NovaBank.Contracts.Limits;

public record CreateLimitRequest(
    LimitType LimitType,
    LimitScope Scope,
    Currency Currency,
    decimal MaxAmount,
    decimal MinAmount = 0,
    Guid? ScopeId = null,
    UserRole? ScopeRole = null,
    decimal? RequiresApprovalAbove = null
);

public record UpdateLimitRequest(
    decimal MaxAmount,
    decimal MinAmount,
    decimal? RequiresApprovalAbove = null,
    bool? IsActive = null
);

public record TransactionLimitResponse(
    Guid Id,
    LimitType LimitType,
    LimitScope Scope,
    Guid? ScopeId,
    UserRole? ScopeRole,
    Currency Currency,
    decimal MaxAmount,
    decimal MinAmount,
    decimal? RequiresApprovalAbove,
    bool IsActive
);
