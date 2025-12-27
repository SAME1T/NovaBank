using Mapster;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Contracts.ApprovalWorkflows;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Exceptions;

namespace NovaBank.Application.ApprovalWorkflows;

public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly IApprovalWorkflowRepository _workflowRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public ApprovalWorkflowService(
        IApprovalWorkflowRepository workflowRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        CurrentUser currentUser)
    {
        _workflowRepository = workflowRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ApprovalWorkflowResponse> CreateRequestAsync(CreateApprovalRequest request, CancellationToken ct = default)
    {
        // 1. Check if authenticated
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        // 2. Check for duplicate pending requests
        var existing = await _workflowRepository.GetPendingForEntityAsync(request.EntityType, request.EntityId, ct);
        if (existing != null)
            throw new InvalidOperationException("This entity already has a pending approval request.");

        // 3. Create workflow
        var workflow = new ApprovalWorkflow(
            request.EntityType,
            request.EntityId,
            _currentUser.CustomerId.Value,
            request.RequiredRole,
            request.Amount,
            request.Currency,
            null, // default expiration
            request.MetadataJson
        );

        await _workflowRepository.AddAsync(workflow, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return workflow.Adapt<ApprovalWorkflowResponse>();
    }

    public async Task<List<ApprovalWorkflowResponse>> GetPendingApprovalsAsync(UserRole? role, CancellationToken ct = default)
    {
        // Only Admin or Manager can see pending approvals usually
        if (!_currentUser.IsAdmin && !_currentUser.IsManager)
             throw new UnauthorizedAccessException("Insufficient permissions.");

        // If role is not specified, use user's role (Manager sees Manager approvals)
        var targetRole = role ?? _currentUser.Role ?? UserRole.Manager;

        var workflows = await _workflowRepository.GetPendingAsync(targetRole, ct);
        return workflows.Adapt<List<ApprovalWorkflowResponse>>();
    }

    public async Task<List<ApprovalWorkflowResponse>> GetMyRequestsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var workflows = await _workflowRepository.GetByRequesterIdAsync(_currentUser.CustomerId.Value, ct);
        return workflows.Adapt<List<ApprovalWorkflowResponse>>();
    }

    public async Task ApproveRequestAsync(Guid id, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id, ct);
        if (workflow == null)
            throw new NotFoundException("Approval request not found.");

        // Validation
        if (workflow.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Request is not pending.");

        if (!_currentUser.IsAdmin && _currentUser.Role != workflow.RequiredRole)
            throw new UnauthorizedAccessException("You do not have the required role to approve this request.");
        
        // Cannot approve own request (Segregation of Duties)
        if (workflow.RequestedById == _currentUser.CustomerId)
            throw new InvalidOperationException("You cannot approve your own request.");

        // Execute Business Logic based on Type
        await ExecuteApprovalActionAsync(workflow, ct);

        // Update Workflow
        workflow.Approve(_currentUser.CustomerId.Value);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RejectRequestAsync(Guid id, string reason, CancellationToken ct = default)
    {
         var workflow = await _workflowRepository.GetByIdAsync(id, ct);
        if (workflow == null)
            throw new NotFoundException("Approval request not found.");

        if (workflow.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Request is not pending.");

        if (!_currentUser.IsAdmin && _currentUser.Role != workflow.RequiredRole)
            throw new UnauthorizedAccessException("You do not have the required role to reject this request.");

        workflow.Reject(_currentUser.CustomerId.Value, reason);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelRequestAsync(Guid id, CancellationToken ct = default)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id, ct);
        if (workflow == null)
            throw new NotFoundException("Approval request not found.");

        if (workflow.RequestedById != _currentUser.CustomerId && !_currentUser.IsAdmin)
             throw new UnauthorizedAccessException("You can only cancel your own requests.");

        workflow.Cancel();
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ExecuteApprovalActionAsync(ApprovalWorkflow workflow, CancellationToken ct)
    {
        switch (workflow.EntityType)
        {
            case ApprovalEntityType.AccountOpening:
                var account = await _accountRepository.GetByIdAsync(workflow.EntityId, ct);
                if (account != null)
                {
                    account.Approve(_currentUser.CustomerId.Value);
                }
                break;
            
            // TODO: Implement other types (Transfer, Loan, etc.)
            case ApprovalEntityType.Transfer:
                // Transfer logic (e.g. execute stored transfer) would go here
                break;
                
            case ApprovalEntityType.LoanApplication:
                // Loan logic
                break;
        }
    }
}
