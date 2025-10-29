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

        g.MapPost("/internal", async Task<Results<Ok<TransferResponse>, BadRequest<string>, NotFound>>
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

            var transfer = new Transfer(from.Id, to.Id, new Money(req.Amount, req.Currency), TransferChannel.Internal);
            db.Transfers.Add(transfer);

            db.Transactions.AddRange(
                new Transaction(from.Id, new Money(req.Amount, req.Currency), TransactionDirection.Debit,  req.Description ?? string.Empty),
                new Transaction(to.Id,   new Money(req.Amount, req.Currency), TransactionDirection.Credit, req.Description ?? string.Empty)
            );
            await db.SaveChangesAsync();
            await trx.CommitAsync();

            return TypedResults.Ok(new TransferResponse(
                transfer.Id, 
                transfer.FromAccountId, 
                transfer.ToAccountId ?? Guid.Empty, 
                transfer.Amount.Amount, 
                transfer.Amount.Currency.ToString(), 
                transfer.Channel.ToString(), 
                transfer.Status.ToString(), 
                transfer.CreatedAt
            ));
        });

        g.MapPost("/external", async Task<Results<Ok<TransferResponse>, BadRequest<string>, NotFound>>
        (TransferExternalRequest req, BankDbContext db) =>
        {
            var from = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.FromAccountId);
            if (from is null) return TypedResults.NotFound();
            if (from.Currency != req.Currency) return TypedResults.BadRequest("Para birimi uyuşmuyor.");
            if (!from.CanWithdraw(new Money(req.Amount, req.Currency))) return TypedResults.BadRequest("Yetersiz bakiye/limit.");

            using var trx = await db.Database.BeginTransactionAsync();
            from.Withdraw(new Money(req.Amount, req.Currency));
            db.Transactions.Add(new Transaction(from.Id, new Money(req.Amount, req.Currency), TransactionDirection.Debit, req.Description ?? string.Empty));

            // IBAN bizim bankamızdaysa alıcıyı da yatır
            Guid? toAccountId = null;
            var internalTo = await db.Accounts.FirstOrDefaultAsync(a => a.Iban == new Iban(req.ToIban));
            if (internalTo is not null)
            {
                if (internalTo.Currency != req.Currency)
                    return TypedResults.BadRequest("Alıcı hesabın para birimi uyuşmuyor.");

                internalTo.Deposit(new Money(req.Amount, req.Currency));
                db.Transactions.Add(new Transaction(internalTo.Id, new Money(req.Amount, req.Currency), TransactionDirection.Credit, req.Description ?? string.Empty));
                toAccountId = internalTo.Id;
            }

            var transfer = new Transfer(from.Id, toAccountId, new Money(req.Amount, req.Currency), TransferChannel.EFT, req.ToIban);
            db.Transfers.Add(transfer);
            await db.SaveChangesAsync();
            await trx.CommitAsync();
            
            return TypedResults.Ok(new TransferResponse(
                transfer.Id, 
                transfer.FromAccountId, 
                transfer.ToAccountId ?? Guid.Empty, 
                transfer.Amount.Amount, 
                transfer.Amount.Currency.ToString(), 
                transfer.Channel.ToString(), 
                transfer.Status.ToString(), 
                transfer.CreatedAt
            ));
        });

        return app;
    }
}
