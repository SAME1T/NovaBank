using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Api.Contracts;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class TransactionsEndpoints
{
    public static IEndpointRouteBuilder MapTransactions(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/transactions");

        g.MapPost("/deposit", async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound>>
        (DepositRequest req, BankDbContext db) =>
        {
            var acc = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.AccountId);
            if (acc is null) return TypedResults.NotFound();
            if (acc.Currency != req.Currency) return TypedResults.BadRequest("Para birimi uyuşmuyor.");

            acc.Deposit(new Money(req.Amount, req.Currency));
            var tx = new Transaction(
                acc.Id,
                new Money(req.Amount, req.Currency),
                TransactionDirection.Credit,
                req.Description ?? string.Empty
            );
            db.Transactions.Add(tx);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new TransactionResponse(tx.Id, tx.AccountId, tx.Amount.Amount, tx.Amount.Currency.ToString(), tx.Direction.ToString(), tx.Description, tx.ReferenceCode, tx.CreatedAt));
        });

        g.MapPost("/withdraw", async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound>>
        (WithdrawRequest req, BankDbContext db) =>
        {
            var acc = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.AccountId);
            if (acc is null) return TypedResults.NotFound();
            if (acc.Currency != req.Currency) return TypedResults.BadRequest("Para birimi uyuşmuyor.");
            if (!acc.CanWithdraw(new Money(req.Amount, req.Currency))) return TypedResults.BadRequest("Bakiye + ek hesap limiti yetersiz.");

            acc.Withdraw(new Money(req.Amount, req.Currency));
            var tx = new Transaction(
                acc.Id,
                new Money(req.Amount, req.Currency),
                TransactionDirection.Debit,
                req.Description ?? string.Empty
            );
            db.Transactions.Add(tx);
            await db.SaveChangesAsync();
            return TypedResults.Ok(new TransactionResponse(tx.Id, tx.AccountId, tx.Amount.Amount, tx.Amount.Currency.ToString(), tx.Direction.ToString(), tx.Description, tx.ReferenceCode, tx.CreatedAt));
        });

        return app;
    }
}
