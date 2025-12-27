using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Contracts.ApprovalWorkflows;

public record CreateApprovalRequest(
    ApprovalEntityType EntityType,
    Guid EntityId,
    decimal? Amount,
    Currency? Currency,
    UserRole RequiredRole = UserRole.Manager,
    string? MetadataJson = null
);

public record ApprovalWorkflowResponse(
    Guid Id,
    ApprovalEntityType EntityType,
    Guid EntityId,
    Guid RequestedById,
    string RequesterName, 
    decimal? Amount,
    Currency? Currency,
    ApprovalStatus Status,
    UserRole RequiredRole,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? ApprovedAt,
    Guid? ApprovedById,
    string? ApproverName,
    string? RejectionReason
);
