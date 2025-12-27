using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Accounts;
using NovaBank.Application.Common.Errors;
using NovaBank.Contracts.Accounts;

namespace NovaBank.Api.Endpoints;
public static class AccountsEndpoints
{
    public static IEndpointRouteBuilder MapAccounts(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/accounts").RequireAuthorization("AnyUser");

        g.MapGet("/", async Task<Results<Ok<List<AccountResponse>>, UnauthorizedHttpResult>> (IAccountsService service) =>
        {
            var result = await service.GetAllAsync();
            if (!result.IsSuccess) return TypedResults.Ok(new List<AccountResponse>()); // veya hata dönülebilir
            return TypedResults.Ok(result.Value!);
        }).RequireAuthorization("AdminOnly");

        g.MapPost("/", async Task<Results<Created<AccountResponse>, BadRequest<string>>> (CreateAccountRequest req, IAccountsService service) =>
        {
            var result = await service.CreateAccountAsync(req);
            if (!result.IsSuccess)
                return TypedResults.BadRequest(result.ErrorMessage ?? "Hesap oluşturulamadı.");

            return TypedResults.Created($"/api/v1/accounts/{result.Value!.Id}", result.Value);
        });

        g.MapGet("/{id:guid}", async Task<Results<Ok<AccountResponse>, NotFound>> (Guid id, IAccountsService service) =>
        {
            var result = await service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/by-customer/{customerId:guid}", async Task<Ok<List<AccountResponse>>> (Guid customerId, IAccountsService service) =>
        {
            var result = await service.GetByCustomerIdAsync(customerId);
            return TypedResults.Ok(result.Value ?? new List<AccountResponse>());
        });

        g.MapGet("/by-account-no/{accountNo:long}", async Task<Results<Ok<AccountResponse>, NotFound>> (long accountNo, IAccountsService service) =>
        {
            var result = await service.GetByAccountNoAsync(accountNo);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/by-iban/{iban}", async Task<Results<Ok<AccountResponse>, NotFound>> (string iban, IAccountsService service) =>
        {
            var result = await service.GetByIbanAsync(iban);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/owner-by-iban/{iban}", async Task<Results<Ok<string>, NotFound>> (string iban, IAccountsService service) =>
        {
            var result = await service.GetOwnerNameByIbanAsync(iban);
            if (!result.IsSuccess)
                return TypedResults.NotFound();

            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}
