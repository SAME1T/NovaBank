using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Api.Contracts;
using NovaBank.Core.Entities;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReports(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/reports");

        g.MapGet("/account-statement", async Task<Results<Ok<AccountStatementResponse>, BadRequest<string>, NotFound>>
        (Guid accountId, DateTime from, DateTime to, BankDbContext db) =>
        {
            if (from > to) return TypedResults.BadRequest("'from' > 'to'");
            var acc = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
            if (acc is null) return TypedResults.NotFound();

            var txs = await db.Transactions
                .Where(t => t.AccountId == accountId && t.TransactionDate >= from && t.TransactionDate <= to)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();

            decimal credit = txs.Where(t => t.Direction.ToString()=="Credit").Sum(t => t.Amount.Amount);
            decimal debit  = txs.Where(t => t.Direction.ToString()=="Debit").Sum(t => t.Amount.Amount);
            decimal closing = acc.Balance.Amount;
            decimal opening = closing - (credit - debit);

            var items = txs.Select(t => new AccountStatementItem(t.TransactionDate, t.Direction.ToString(), t.Amount.Amount, t.Amount.Currency.ToString(), t.Description, t.ReferenceCode)).ToList();
            return TypedResults.Ok(new AccountStatementResponse(accountId, from, to, opening, credit, debit, closing, items));
        });

        g.MapGet("/customer-summary", async Task<Results<Ok<CustomerSummaryResponse>, NotFound>>
        (Guid customerId, BankDbContext db) =>
        {
            var cust = await db.Customers.FindAsync(customerId);
            if (cust is null) return TypedResults.NotFound();

            var accounts = await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync();
            var cards    = await db.Cards.CountAsync(c => accounts.Select(a => a.Id).Contains(c.AccountId));
            var loans    = await db.Loans.CountAsync(l => l.CustomerId == customerId);
            var totalTry = accounts.Sum(a => a.Balance.Amount);

            return TypedResults.Ok(new CustomerSummaryResponse(customerId, $"{cust.FirstName} {cust.LastName}", accounts.Count, totalTry, cards, loans));
        });

        return app;
    }
}
