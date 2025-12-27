using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.Commissions;
using NovaBank.Contracts.Commissions;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Endpoints;

public static class CommissionsEndpoints
{
    public static void MapCommissions(this WebApplication app)
    {
        var group = app.MapGroup("/api/commissions").WithTags("Commissions");

        group.MapPost("/", async ([FromBody] CreateCommissionRequest request, ICommissionService service, CancellationToken ct) =>
        {
            var result = await service.CreateCommissionAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser"); // Service checks for Admin

        group.MapGet("/", async (CommissionType type, ICommissionService service, CancellationToken ct) =>
        {
            var result = await service.GetActiveCommissionsAsync(type, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateCommissionRequest request, ICommissionService service, CancellationToken ct) =>
        {
            await service.UpdateCommissionAsync(id, request, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser"); // Service checks for Admin
    }
}
