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
        var g = app.MapGroup("/api/v1/admin").RequireAuthorization("AdminOnly");

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

        g.MapPut("/accounts/{accountId:guid}/status", async Task<Results<Ok, BadRequest<string>, NotFound, Conflict<string>, UnauthorizedHttpResult>>
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
                    ErrorCodes.AccountNotFound => TypedResults.NotFound(),
                    ErrorCodes.KapatmakIcinBakiyeSifirOlmali => TypedResults.BadRequest(result.ErrorMessage ?? "Hesabı kapatmak için bakiye sıfır olmalı."),
                    ErrorCodes.KapaliHesapTekrarAcilamaz => TypedResults.Conflict(result.ErrorMessage ?? "Kapalı hesap tekrar açılamaz."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Hesap durumu güncellenemedi.")
                };
            }
            return TypedResults.Ok();
        });

        g.MapPut("/customers/{customerId:guid}/active", async Task<Results<Ok<UpdateCustomerActiveResponse>, BadRequest<string>, NotFound, StatusCodeHttpResult>>
        (Guid customerId, UpdateCustomerActiveRequest req, IAdminService service) =>
        {
            var result = await service.UpdateCustomerActiveAsync(customerId, req.IsActive);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.StatusCode(403),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri aktiflik durumu güncellenemedi.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        g.MapPost("/customers/{customerId:guid}/reset-password", async Task<Results<Ok<ResetCustomerPasswordResponse>, BadRequest<string>, NotFound, StatusCodeHttpResult>>
        (Guid customerId, IAdminService service) =>
        {
            var result = await service.ResetCustomerPasswordAsync(customerId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.StatusCode(403),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Şifre sıfırlama başarısız.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        g.MapGet("/audit-logs", async Task<Results<Ok<List<AuditLogResponse>>, BadRequest<string>, StatusCodeHttpResult>>
        (string? from, string? to, string? search, string? action, string? success, int? take, IAdminService service) =>
        {
            // DateTime parse et - ISO 8601 formatında UTC olarak parse et
            DateTime? fromDt = null;
            if (!string.IsNullOrWhiteSpace(from))
            {
                if (DateTime.TryParse(from, null, System.Globalization.DateTimeStyles.RoundtripKind, out var f))
                {
                    // Unspecified gelirse Local kabul edip UTC'ye çevir
                    if (f.Kind == DateTimeKind.Unspecified)
                        f = DateTime.SpecifyKind(f, DateTimeKind.Local);
                    
                    // UTC'ye çevir (PostgreSQL timestamptz için gerekli)
                    fromDt = f.Kind == DateTimeKind.Utc ? f : f.ToUniversalTime();
                }
            }
            
            DateTime? toDt = null;
            if (!string.IsNullOrWhiteSpace(to))
            {
                if (DateTime.TryParse(to, null, System.Globalization.DateTimeStyles.RoundtripKind, out var t))
                {
                    // Unspecified gelirse Local kabul edip UTC'ye çevir
                    if (t.Kind == DateTimeKind.Unspecified)
                        t = DateTime.SpecifyKind(t, DateTimeKind.Local);
                    
                    // UTC'ye çevir (PostgreSQL timestamptz için gerekli)
                    toDt = t.Kind == DateTimeKind.Utc ? t : t.ToUniversalTime();
                }
            }
            
            // bool parse et
            bool? successBool = null;
            if (success == "true")
                successBool = true;
            else if (success == "false")
                successBool = false;
            // null veya başka bir değer ise successBool = null kalır
            
            // take değerini clamp et
            var takeValue = take.HasValue ? Math.Clamp(take.Value, 1, 1000) : 200;
            
            var result = await service.GetAuditLogsAsync(fromDt, toDt, search, action, successBool, takeValue);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.StatusCode(403),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Audit logları alınamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}

