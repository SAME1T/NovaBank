namespace NovaBank.Application.Common.Interfaces;

/// <summary>
/// Audit logging interface for tracking system actions.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(
        string action,
        bool success,
        string? entityType = null,
        string? entityId = null,
        string? summary = null,
        object? metadata = null,
        string? errorCode = null,
        CancellationToken ct = default);
}

