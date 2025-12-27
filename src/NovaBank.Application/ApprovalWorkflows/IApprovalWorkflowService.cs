using NovaBank.Contracts.ApprovalWorkflows;
using NovaBank.Core.Enums;

namespace NovaBank.Application.ApprovalWorkflows;

public interface IApprovalWorkflowService
{
    Task<ApprovalWorkflowResponse> CreateRequestAsync(CreateApprovalRequest request, CancellationToken ct = default);
    Task<List<ApprovalWorkflowResponse>> GetPendingApprovalsAsync(UserRole? role, CancellationToken ct = default);
    Task<List<ApprovalWorkflowResponse>> GetMyRequestsAsync(CancellationToken ct = default);
    Task ApproveRequestAsync(Guid id, CancellationToken ct = default);
    Task RejectRequestAsync(Guid id, string reason, CancellationToken ct = default);
    Task CancelRequestAsync(Guid id, CancellationToken ct = default);
}
