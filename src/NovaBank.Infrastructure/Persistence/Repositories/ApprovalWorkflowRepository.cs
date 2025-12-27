using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class ApprovalWorkflowRepository : IApprovalWorkflowRepository
{
    private readonly BankDbContext _context;

    public ApprovalWorkflowRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ApprovalWorkflows
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<ApprovalWorkflow>> GetPendingAsync(UserRole role, CancellationToken ct = default)
    {
        return await _context.ApprovalWorkflows
            .Where(x => x.Status == ApprovalStatus.Pending && x.RequiredRole == role)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ApprovalWorkflow>> GetByRequesterIdAsync(Guid requesterId, CancellationToken ct = default)
    {
        return await _context.ApprovalWorkflows
            .Where(x => x.RequestedById == requesterId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ApprovalWorkflow workflow, CancellationToken ct = default)
    {
        await _context.ApprovalWorkflows.AddAsync(workflow, ct);
    }

    public async Task<ApprovalWorkflow?> GetPendingForEntityAsync(ApprovalEntityType entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _context.ApprovalWorkflows
            .FirstOrDefaultAsync(x => 
                x.EntityType == entityType && 
                x.EntityId == entityId && 
                x.Status == ApprovalStatus.Pending, ct);
    }
}
