using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Contracts.PaymentOrders;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class PaymentOrdersEndpoints
{
    public static IEndpointRouteBuilder MapPaymentOrders(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/payment-orders").RequireAuthorization("AnyUser");

        g.MapPost("/", async Task<Results<Created<PaymentOrderResponse>, BadRequest<string>, NotFound>>
        (CreatePaymentOrderRequest req, BankDbContext db) =>
        {
            var acc = await db.Accounts.FindAsync(req.AccountId);
            if (acc is null) return TypedResults.NotFound();
            if (acc.Currency != req.Currency) return TypedResults.BadRequest("Para birimi uyuşmuyor.");

            var po = new PaymentOrder(
                acc.Id,
                req.PayeeName,
                new Iban(req.PayeeIban),
                new Money(req.Amount, req.Currency),
                req.CronExpr,
                DateTime.UtcNow.AddDays(1)
            );
            db.PaymentOrders.Add(po);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/v1/payment-orders/{po.Id}",
                new PaymentOrderResponse(po.Id, po.AccountId, po.PayeeName, po.PayeeIban.Value, po.Amount.Amount, po.Amount.Currency.ToString(), po.CronExpr, po.Status.ToString(), po.NextRunAt));
        });

        return app;
    }
}
