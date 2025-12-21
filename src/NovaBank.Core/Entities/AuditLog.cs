using NovaBank.Core.Abstractions;

namespace NovaBank.Core.Entities;

/// <summary>
/// Audit log entity for tracking system actions.
/// </summary>
public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public Guid? ActorCustomerId { get; private set; }
    public string ActorRole { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? Summary { get; private set; }
    public string? MetadataJson { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorCode { get; private set; }

    public AuditLog(
        Guid? actorCustomerId,
        string actorRole,
        string action,
        bool success,
        string? entityType = null,
        string? entityId = null,
        string? summary = null,
        string? metadataJson = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorCode = null)
    {
        ActorCustomerId = actorCustomerId;
        ActorRole = actorRole ?? throw new ArgumentNullException(nameof(actorRole));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Success = success;
        EntityType = entityType;
        EntityId = entityId;
        Summary = summary;
        MetadataJson = metadataJson;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ErrorCode = errorCode;
    }
}

