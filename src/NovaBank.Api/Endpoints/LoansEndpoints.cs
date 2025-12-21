using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Contracts.Loans;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class LoansEndpoints
{
    public static IEndpointRouteBuilder MapLoans(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/loans").RequireAuthorization("AnyUser");

        // Hesaplama
        g.MapPost("/calc", (CalcLoanRequest req) =>
        {
            var r = req.InterestRateAnnual / 12m;
            var n = req.TermMonths;
            var P = req.Principal;
            var A = r == 0 ? P / n : P * r / (1 - (decimal)Math.Pow(1 + (double)r, -n));
            var total = A * n;
            var interest = total - P;
            return TypedResults.Ok(new CalcLoanResponse(decimal.Round(A,2), decimal.Round(total,2), decimal.Round(interest,2)));
        });

        // Başvuru
        g.MapPost("/apply", async Task<Results<Created<LoanResponse>, BadRequest<string>, NotFound>>
        (ApplyLoanRequest req, BankDbContext db) =>
        {
            var cust = await db.Customers.FindAsync(req.CustomerId);
            if (cust is null) return TypedResults.NotFound();

            var loan = new Loan(
                req.CustomerId,
                new Money(req.Principal, req.Currency),
                req.InterestRateAnnual,
                req.TermMonths,
                req.StartDate
            );
            // Aktivasyon örnek
            loan.Close();
            loan.Default();
            db.Loans.Add(loan);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/v1/loans/{loan.Id}",
                new LoanResponse(loan.Id, loan.CustomerId, loan.Principal.Amount, loan.Principal.Currency.ToString(), loan.InterestRateAnnual, loan.TermMonths, loan.StartDate, loan.Status.ToString()));
        });

        return app;
    }
}
