using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Customers;
using NovaBank.Contracts.Customers;

namespace NovaBank.Api.Endpoints;
public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/customers");

        g.MapPost("/", async Task<Results<Created<CustomerResponse>, BadRequest<string>>> (CreateCustomerRequest req, ICustomersService service) =>
        {
            var result = await service.CreateCustomerAsync(req);
            if (!result.IsSuccess)
                return TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri oluşturulamadı.");

            return TypedResults.Created($"/api/v1/customers/{result.Value!.Id}", result.Value);
        });

        g.MapGet("/{id:guid}", async Task<Results<Ok<CustomerResponse>, NotFound>> (Guid id, ICustomersService service) =>
        {
            var result = await service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/", async Task<Ok<List<CustomerResponse>>> (ICustomersService service) =>
        {
            var result = await service.GetAllAsync();
            return TypedResults.Ok(result.Value ?? new List<CustomerResponse>());
        });

        g.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest<string>>> (LoginRequest req, ICustomersService service) =>
        {
            var result = await service.LoginAsync(req);
            if (!result.IsSuccess)
                return TypedResults.BadRequest(result.ErrorMessage ?? "Giriş başarısız.");

            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}
