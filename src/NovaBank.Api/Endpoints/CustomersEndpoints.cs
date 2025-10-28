using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Api.Contracts;
using NovaBank.Core.Entities;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;
public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/customers");

        g.MapPost("/", async Task<Results<Created<CustomerResponse>, BadRequest<string>>> (CreateCustomerRequest req, BankDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
                return TypedResults.BadRequest("FirstName/LastName boş olamaz.");

            var exists = await db.Customers.AnyAsync(c => c.NationalId == new NationalId(req.NationalId));
            if (exists) return TypedResults.BadRequest("NationalId zaten kayıtlı.");

            var c = new Customer(
                new NationalId(req.NationalId),
                req.FirstName,
                req.LastName,
                req.Email ?? string.Empty,
                req.Phone ?? string.Empty,
                req.Password
            );
            db.Customers.Add(c);
            await db.SaveChangesAsync();

            var dto = new CustomerResponse(c.Id, c.NationalId.Value, c.FirstName, c.LastName, c.Email, c.Phone, c.IsActive);
            return TypedResults.Created($"/api/v1/customers/{c.Id}", dto);
        });

        g.MapGet("/{id:guid}", async Task<Results<Ok<CustomerResponse>, NotFound>> (Guid id, BankDbContext db) =>
        {
            var c = await db.Customers.FindAsync(id);
            if (c is null) return TypedResults.NotFound();
            var dto = new CustomerResponse(c.Id, c.NationalId.Value, c.FirstName, c.LastName, c.Email, c.Phone, c.IsActive);
            return TypedResults.Ok(dto);
        });

        g.MapPost("/login", async Task<Results<Ok<CustomerResponse>, BadRequest<string>>> (LoginRequest req, BankDbContext db) =>
        {
            var c = await db.Customers.FirstOrDefaultAsync(x => x.NationalId == new NationalId(req.NationalId));
            if (c is null) return TypedResults.BadRequest("Kullanıcı bulunamadı.");
            if (!c.VerifyPassword(req.Password)) return TypedResults.BadRequest("Şifre hatalı.");
            if (!c.IsActive) return TypedResults.BadRequest("Hesap deaktif.");
            return TypedResults.Ok(new CustomerResponse(c.Id, c.NationalId.Value, c.FirstName, c.LastName, c.Email, c.Phone, c.IsActive));
        });

        return app;
    }
}
