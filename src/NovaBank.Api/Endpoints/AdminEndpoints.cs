using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Admin;
using NovaBank.Application.Common.Errors;
using NovaBank.Contracts.Admin;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdmin(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/admin");

        g.MapGet("/customers", async Task<Results<Ok<List<CustomerSummaryResponse>>, BadRequest<string>, UnauthorizedHttpResult>>
        (string? search, IAdminService service) =>
        {
            var result = await service.SearchCustomersAsync(search);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri arama başarısız.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/customers/{customerId:guid}/accounts", async Task<Results<Ok<List<AccountAdminResponse>>, BadRequest<string>, UnauthorizedHttpResult>>
        (Guid customerId, IAdminService service) =>
        {
            var result = await service.GetCustomerAccountsAsync(customerId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Hesaplar alınamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        g.MapPut("/accounts/{accountId:guid}/overdraft", async Task<Results<Ok, BadRequest<string>, UnauthorizedHttpResult>>
        (Guid accountId, UpdateOverdraftRequest req, IAdminService service) =>
        {
            var result = await service.UpdateOverdraftLimitAsync(accountId, req.OverdraftLimit);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.Validation => TypedResults.BadRequest(result.ErrorMessage ?? "Geçersiz istek."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Overdraft limit güncellenemedi.")
                };
            }
            return TypedResults.Ok();
        });

        g.MapPut("/accounts/{accountId:guid}/status", async Task<Results<Ok, BadRequest<string>, UnauthorizedHttpResult>>
        (Guid accountId, UpdateAccountStatusRequest req, IAdminService service) =>
        {
            if (!Enum.TryParse<AccountStatus>(req.Status, out var status))
                return TypedResults.BadRequest("Geçersiz status değeri.");

            var result = await service.UpdateAccountStatusAsync(accountId, status);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Hesap durumu güncellenemedi.")
                };
            }
            return TypedResults.Ok();
        });

        return app;
    }
}

