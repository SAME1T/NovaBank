using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.Kyc;
using NovaBank.Contracts.Kyc;

namespace NovaBank.Api.Endpoints;

public static class KycEndpoints
{
    public static void MapKyc(this WebApplication app)
    {
        var group = app.MapGroup("/api/kyc").WithTags("KYC");

        // Customer submits verification
        group.MapPost("/", async ([FromBody] CreateKycVerificationRequest request, IKycService service, CancellationToken ct) =>
        {
            var result = await service.SubmitVerificationAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Customer gets own verifications
        group.MapGet("/my", async (IKycService service, CancellationToken ct) =>
        {
            var result = await service.GetMyVerificationsAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Admin/Manager gets pending
        group.MapGet("/pending", async (IKycService service, CancellationToken ct) =>
        {
            var result = await service.GetPendingVerificationsAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser"); // Service checks logic

        // Verify
        group.MapPost("/{id}/verify", async (Guid id, IKycService service, CancellationToken ct) =>
        {
            await service.VerifyAsync(id, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser");

        // Reject
        group.MapPost("/{id}/reject", async (Guid id, [FromQuery] string reason, IKycService service, CancellationToken ct) =>
        {
            await service.RejectAsync(id, reason, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser");
    }
}
