using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.Notifications;
using NovaBank.Contracts.Notifications;

namespace NovaBank.Api.Endpoints;

public static class NotificationsEndpoints
{
    public static void MapNotifications(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");

        // My Notifications
        group.MapGet("/", async ([FromQuery] int take, INotificationService service, CancellationToken ct) =>
        {
            var result = await service.GetMyNotificationsAsync(take <= 0 ? 50 : take, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Unread Count
        group.MapGet("/unread-count", async (INotificationService service, CancellationToken ct) =>
        {
            var result = await service.GetUnreadCountAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Mark Read
        group.MapPost("/{id}/read", async (Guid id, INotificationService service, CancellationToken ct) =>
        {
            await service.MarkAsReadAsync(id, ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser");

        // Mark All Read
        group.MapPost("/read-all", async (INotificationService service, CancellationToken ct) =>
        {
            await service.MarkAllAsReadAsync(ct);
            return Results.Ok();
        }).RequireAuthorization("AnyUser");

        // Send (Admin)
        group.MapPost("/send", async ([FromBody] CreateNotificationRequest request, INotificationService service, CancellationToken ct) =>
        {
            var result = await service.SendNotificationAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser"); // Service checks for Admin
    }
}
