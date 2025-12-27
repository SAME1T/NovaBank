using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.ApprovalWorkflows;
using NovaBank.Contracts.ApprovalWorkflows;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Endpoints;

public static class ApprovalWorkflowsEndpoints
{
    public static void MapApprovalWorkflows(this WebApplication app)
    {
        var group = app.MapGroup("/api/approval-workflows").WithTags("Approval Workflows");

        // Manager/Admin: Get Pending
        group.MapGet("/pending", async (UserRole? role, IApprovalWorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetPendingApprovalsAsync(role, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser"); // Service checks for Manager/Admin role

        // My Requests
        group.MapGet("/my-requests", async (IApprovalWorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetMyRequestsAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Create Request (Usually internal via other services, but exposing for testing)
        group.MapPost("/", async ([FromBody] CreateApprovalRequest request, IApprovalWorkflowService service, CancellationToken ct) =>
        {
            var result = await service.CreateRequestAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Approve
        group.MapPost("/{id}/approve", async (Guid id, IApprovalWorkflowService service, CancellationToken ct) =>
        {
            await service.ApproveRequestAsync(id, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser"); // Service checks for Manager/Admin

        // Reject
        group.MapPost("/{id}/reject", async (Guid id, [FromQuery] string reason, IApprovalWorkflowService service, CancellationToken ct) =>
        {
            await service.RejectRequestAsync(id, reason, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser");

        // Cancel
        group.MapPost("/{id}/cancel", async (Guid id, IApprovalWorkflowService service, CancellationToken ct) =>
        {
            await service.CancelRequestAsync(id, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser");
    }
}
