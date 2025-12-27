using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.Bills;
using NovaBank.Contracts.Bills;

namespace NovaBank.Api.Endpoints;

public static class BillsEndpoints
{
    public static void MapBills(this WebApplication app)
    {
        var group = app.MapGroup("/api/bills").WithTags("Bills");

        // Institutions
        group.MapGet("/institutions", async (IBillPaymentService service, CancellationToken ct) =>
        {
            var result = await service.GetInstitutionsAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Inquiry
        group.MapPost("/inquire", async ([FromBody] BillInquiryRequest request, IBillPaymentService service, CancellationToken ct) =>
        {
            var result = await service.InquireAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Pay
        group.MapPost("/pay", async ([FromBody] PayBillRequest request, IBillPaymentService service, CancellationToken ct) =>
        {
            var result = await service.PayAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // History by Account
        group.MapGet("/history/{accountId}", async (Guid accountId, IBillPaymentService service, CancellationToken ct) =>
        {
            var result = await service.GetHistoryAsync(accountId, ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // My Full History
        group.MapGet("/my-history", async (IBillPaymentService service, CancellationToken ct) =>
        {
            var result = await service.GetCustomerHistoryAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization("AnyUser");

        // Admin: Manage Institutions
        group.MapPost("/institutions", async ([FromBody] CreateBillInstitutionRequest request, IBillPaymentService service, CancellationToken ct) =>
        {
            var result = await service.CreateInstitutionAsync(request, ct);
            return Results.Created($"/api/bills/institutions/{result.Id}", result);
        }).RequireAuthorization("AdminOnly");

        group.MapDelete("/institutions/{id}", async (Guid id, IBillPaymentService service, CancellationToken ct) =>
        {
            await service.DeleteInstitutionAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("AdminOnly");
    }
}
