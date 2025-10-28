using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Api.Contracts;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class CardsEndpoints
{
    public static IEndpointRouteBuilder MapCards(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/cards");

        // Kart bas (debit/credit)
        g.MapPost("/", async Task<Results<Created<CardResponse>, BadRequest<string>, NotFound>>
        (IssueCardRequest req, BankDbContext db) =>
        {
            var acc = await db.Accounts.FirstOrDefaultAsync(a => a.Id == req.AccountId);
            if (acc is null) return TypedResults.NotFound();

            // Basit PAN maskesi üretimi (sahte, demo)
            var last4 = Random.Shared.Next(1000, 9999).ToString();
            var masked = $"**** **** **** {last4}";
            var month = Random.Shared.Next(1, 12);
            var year = DateTime.UtcNow.Year % 100 + 3; // +3 yıl

            var card = new Card(
                acc.Id,
                req.CardType,
                masked,
                month,
                year,
                req.CardType == CardType.Credit ? Math.Max(0, req.CreditLimit ?? 0) : null,
                req.CardType == CardType.Credit ? Math.Max(0, req.CreditLimit ?? 0) : null
            );
            db.Cards.Add(card);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/v1/cards/{card.Id}",
                new CardResponse(card.Id, card.AccountId, card.MaskedPan, card.ExpiryMonth, card.ExpiryYear,
                                 card.CardType.ToString(), card.CardStatus.ToString(), card.CreditLimit, card.AvailableLimit));
        });

        // Bloke / Bloke kaldır
        g.MapPost("/block", async Task<Results<Ok<string>, NotFound>> (CardBlockRequest req, BankDbContext db) =>
        {
            var card = await db.Cards.FindAsync(req.CardId);
            if (card is null) return TypedResults.NotFound();
            card.Block();
            await db.SaveChangesAsync();
            return TypedResults.Ok("Kart bloke edildi.");
        });

        g.MapPost("/unblock", async Task<Results<Ok<string>, NotFound>> (CardUnblockRequest req, BankDbContext db) =>
        {
            var card = await db.Cards.FindAsync(req.CardId);
            if (card is null) return TypedResults.NotFound();
            card.Unblock();
            await db.SaveChangesAsync();
            return TypedResults.Ok("Kart blokesu kaldırıldı.");
        });

        return app;
    }
}
