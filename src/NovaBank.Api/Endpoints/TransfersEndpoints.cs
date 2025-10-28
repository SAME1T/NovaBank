using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Api.Contracts;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class TransfersEndpoints
{
    public static IEndpointRouteBuilder MapTransfers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/transfers");

        g.MapPost("/internal", async Task<Results<Ok<string>, BadRequest<string>, NotFound>>
        (TransferInternalRequest req, BankDbContext db) =>
        {
            if (req.FromAccountId == req.ToAccountId) return TypedResults.BadRequest("Aynı hesaba transfer olmaz.");
            var from = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.FromAccountId);
            var to   = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.ToAccountId);
            if (from is null || to is null) return TypedResults.NotFound();
            if (from.Currency != to.Currency || from.Currency != req.Currency) return TypedResults.BadRequest("Para birimi uyuşmuyor.");
            if (!from.CanWithdraw(new Money(req.Amount, req.Currency))) return TypedResults.BadRequest("Yetersiz bakiye/limit.");

            using var trx = await db.Database.BeginTransactionAsync();
            from.Withdraw(new Money(req.Amount, req.Currency));
            to.Deposit(new Money(req.Amount, req.Currency));

            db.Transactions.AddRange(
                new Transaction(from.Id, new Money(req.Amount, req.Currency), TransactionDirection.Debit,  req.Description ?? string.Empty),
                new Transaction(to.Id,   new Money(req.Amount, req.Currency), TransactionDirection.Credit, req.Description ?? string.Empty)
            );
            await db.SaveChangesAsync();
            await trx.CommitAsync();

            return TypedResults.Ok("Transfer tamam.");
        });

        g.MapPost("/external", async Task<Results<Ok<string>, BadRequest<string>, NotFound>>
        (TransferExternalRequest req, BankDbContext db) =>
        {
            var from = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.FromAccountId);
            if (from is null) return TypedResults.NotFound();
            if (from.Currency != req.Currency) return TypedResults.BadRequest("Para birimi uyuşmuyor.");
            if (!from.CanWithdraw(new Money(req.Amount, req.Currency))) return TypedResults.BadRequest("Yetersiz bakiye/limit.");

            using var trx = await db.Database.BeginTransactionAsync();
            from.Withdraw(new Money(req.Amount, req.Currency));
            db.Transactions.Add(new Transaction(from.Id, new Money(req.Amount, req.Currency), TransactionDirection.Debit, req.Description ?? string.Empty));
            db.Transfers.Add(new Transfer(from.Id, null, new Money(req.Amount, req.Currency), TransferChannel.EFT, req.ToIban));
            var lastTransfer = await db.Transfers.OrderByDescending(t => t.CreatedAt).FirstAsync();
            lastTransfer.MarkExecuted();
            await db.SaveChangesAsync();
            await trx.CommitAsync();
            return TypedResults.Ok("EFT/FAST talimatı kaydedildi.");
        });

        return app;
    }
}
