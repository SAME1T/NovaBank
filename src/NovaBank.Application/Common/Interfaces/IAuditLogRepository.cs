using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> QueryAsync(
        DateTime? from,
        DateTime? to,
        string? search,
        string? action,
        bool? success,
        int take,
        CancellationToken ct = default);
}

