using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Common.Interfaces;

public interface IApprovalWorkflowRepository
{
    Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ApprovalWorkflow>> GetPendingAsync(UserRole role, CancellationToken ct = default);
    Task<List<ApprovalWorkflow>> GetByRequesterIdAsync(Guid requesterId, CancellationToken ct = default);
    Task AddAsync(ApprovalWorkflow workflow, CancellationToken ct = default);
    
    // Entity bazlı sorgulama için
    Task<ApprovalWorkflow?> GetPendingForEntityAsync(ApprovalEntityType entityType, Guid entityId, CancellationToken ct = default);
}
