using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.Limits;
using NovaBank.Contracts.Limits;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Endpoints;

public static class LimitsEndpoints
{
    public static void MapLimits(this WebApplication app)
    {
        var group = app.MapGroup("/api/limits").WithTags("Limits");

        group.MapPost("/", async ([FromBody] CreateLimitRequest request, ILimitService service, CancellationToken ct) =>
        {
            var result = await service.CreateLimitAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser"); // Service checks for Admin

        group.MapGet("/", async (ILimitService service, CancellationToken ct) =>
        {
            var result = await service.GetActiveLimitsAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateLimitRequest request, ILimitService service, CancellationToken ct) =>
        {
            await service.UpdateLimitAsync(id, request, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser"); // Service checks for Admin

        group.MapDelete("/{id}", async (Guid id, ILimitService service, CancellationToken ct) =>
        {
            await service.DeactivateLimitAsync(id, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser"); // Service checks for Admin
    }
}
