using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Api.Contracts;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Services;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class AccountsEndpoints
{
    public static IEndpointRouteBuilder MapAccounts(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/accounts");

        g.MapPost("/", async Task<Results<Created<AccountResponse>, BadRequest<string>>> (CreateAccountRequest req, BankDbContext db, IIbanGenerator ibanGenerator) =>
        {
            var customer = await db.Customers.FindAsync(req.CustomerId);
            if (customer is null) return TypedResults.BadRequest("Customer bulunamadı.");

            if (await db.Accounts.AnyAsync(a => a.AccountNo == new AccountNo(req.AccountNo)))
                return TypedResults.BadRequest("AccountNo mevcut.");

            // Otomatik IBAN oluştur
            string generatedIban;
            do
            {
                generatedIban = ibanGenerator.GenerateIban();
            } while (await db.Accounts.AnyAsync(a => a.Iban == new Iban(generatedIban)));

            var acc = new Account(
                req.CustomerId,
                new AccountNo(req.AccountNo),
                new Iban(generatedIban),
                req.Currency,
                new Money(0m, req.Currency),
                Math.Max(0, req.OverdraftLimit)
            );

            db.Accounts.Add(acc);
            await db.SaveChangesAsync();

            var dto = new AccountResponse(acc.Id, acc.CustomerId, acc.AccountNo.Value, acc.Iban.Value, acc.Currency.ToString(), acc.Balance.Amount, acc.OverdraftLimit);
            return TypedResults.Created($"/api/v1/accounts/{acc.Id}", dto);
        });

        g.MapGet("/{id:guid}", async Task<Results<Ok<AccountResponse>, NotFound>> (Guid id, BankDbContext db) =>
        {
            var a = await db.Accounts.FindAsync(id);
            if (a is null) return TypedResults.NotFound();
            var dto = new AccountResponse(a.Id, a.CustomerId, a.AccountNo.Value, a.Iban.Value, a.Currency.ToString(), a.Balance.Amount, a.OverdraftLimit);
            return TypedResults.Ok(dto);
        });

        g.MapGet("/by-customer/{customerId:guid}", async Task<Ok<List<AccountResponse>>> (Guid customerId, BankDbContext db) =>
        {
            var list = await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync();
            return TypedResults.Ok(list.Select(a => new AccountResponse(a.Id, a.CustomerId, a.AccountNo.Value, a.Iban.Value, a.Currency.ToString(), a.Balance.Amount, a.OverdraftLimit)).ToList());
        });

        g.MapGet("/by-account-no/{accountNo:long}", async Task<Results<Ok<AccountResponse>, NotFound>> (long accountNo, BankDbContext db) =>
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.AccountNo == new AccountNo(accountNo));
            if (account is null) return TypedResults.NotFound();
            var dto = new AccountResponse(account.Id, account.CustomerId, account.AccountNo.Value, account.Iban.Value, account.Currency.ToString(), account.Balance.Amount, account.OverdraftLimit);
            return TypedResults.Ok(dto);
        });

        g.MapGet("/by-iban/{iban}", async Task<Results<Ok<AccountResponse>, NotFound>> (string iban, BankDbContext db) =>
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Iban == new Iban(iban));
            if (account is null) return TypedResults.NotFound();
            var dto = new AccountResponse(account.Id, account.CustomerId, account.AccountNo.Value, account.Iban.Value, account.Currency.ToString(), account.Balance.Amount, account.OverdraftLimit);
            return TypedResults.Ok(dto);
        });

        // IBAN'dan hesap sahibinin ad-soyad bilgisini getir
        g.MapGet("/owner-by-iban/{iban}", async Task<Results<Ok<string>, NotFound>> (string iban, BankDbContext db) =>
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Iban == new Iban(iban));
            if (account is null) return TypedResults.NotFound();
            var cust = await db.Customers.FirstOrDefaultAsync(c => c.Id == account.CustomerId);
            if (cust is null) return TypedResults.NotFound();
            var fullName = $"{cust.FirstName} {cust.LastName}";
            return TypedResults.Ok(fullName);
        });

        return app;
    }
}
