using NovaBank.Core.Enums;

namespace NovaBank.Contracts.Commissions;

public record CreateCommissionRequest(
    CommissionType CommissionType,
    string Name,
    Currency Currency,
    decimal FixedAmount,
    decimal PercentageRate,
    decimal MinAmount = 0,
    decimal? MaxAmount = null,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null
);

public record UpdateCommissionRequest(
    string Name,
    decimal FixedAmount,
    decimal PercentageRate,
    decimal MinAmount,
    decimal? MaxAmount,
    string? Description,
    DateTime? ValidUntil,
    bool? IsActive = null
);

public record CommissionResponse(
    Guid Id,
    CommissionType CommissionType,
    string Name,
    string? Description,
    Currency Currency,
    decimal FixedAmount,
    decimal PercentageRate,
    decimal MinAmount,
    decimal? MaxAmount,
    bool IsActive,
    DateTime ValidFrom,
    DateTime? ValidUntil
);
