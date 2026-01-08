using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Admin;
using NovaBank.Application.Common.Errors;
using NovaBank.Contracts.Admin;
using NovaBank.Core.Enums;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdmin(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/admin").RequireAuthorization("AdminOrBranchManager");

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
            
            // Action filtresi: "Hepsi" veya boş ise null gönder
            string? actionFilter = null;
            if (!string.IsNullOrWhiteSpace(action) && action != "Hepsi")
            {
                actionFilter = action;
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
            
            var result = await service.GetAuditLogsAsync(fromDt, toDt, search, actionFilter, successBool, takeValue);
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

        g.MapGet("/pending-approvals", async Task<Results<Ok<List<PendingApprovalResponse>>, BadRequest<string>, UnauthorizedHttpResult>>
        (IAdminService service) =>
        {
            var result = await service.GetPendingApprovalsAsync();
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Onay bekleyen müşteriler alınamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        g.MapPost("/customers/{customerId:guid}/approve", async Task<Results<Ok<ApproveCustomerResponse>, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid customerId, IAdminService service) =>
        {
            var result = await service.ApproveCustomerAsync(customerId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri onaylanamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        g.MapPost("/customers/{customerId:guid}/reject", async Task<Results<Ok, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid customerId, IAdminService service) =>
        {
            var result = await service.RejectCustomerAsync(customerId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri reddedilemedi.")
                };
            }
            return TypedResults.Ok();
        });

        // BranchManager oluşturma endpoint'i (SADECE ADMIN)
        g.MapPost("/branch-managers", async Task<Results<Ok<CreateBranchManagerResponse>, BadRequest<string>, Conflict<string>, UnauthorizedHttpResult>>
        (CreateBranchManagerRequest req, IAdminService service) =>
        {
            var result = await service.CreateBranchManagerAsync(req);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.Conflict => TypedResults.Conflict(result.ErrorMessage ?? "Bu TC Kimlik No ile kayıtlı bir kullanıcı zaten var."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "BranchManager oluşturulamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        // Kullanıcı rolü güncelleme endpoint'i (SADECE ADMIN)
        g.MapPut("/customers/{customerId:guid}/role", async Task<Results<Ok<UpdateCustomerRoleResponse>, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid customerId, UpdateCustomerRoleRequest req, IAdminService service) =>
        {
            if (!Enum.TryParse<UserRole>(req.Role, out var role))
                return TypedResults.BadRequest("Geçersiz rol değeri.");

            var result = await service.UpdateCustomerRoleAsync(customerId, role);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Rol güncellenemedi.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        // Hesap silme endpoint'i (SADECE ADMIN)
        g.MapDelete("/accounts/{accountId:guid}", async Task<Results<Ok, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid accountId, IAdminService service) =>
        {
            var result = await service.DeleteAccountAsync(accountId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Hesap silinemedi.")
                };
            }
            return TypedResults.Ok();
        });

        // Müşteri silme endpoint'i (SADECE ADMIN)
        g.MapDelete("/customers/{customerId:guid}", async Task<Results<Ok, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid customerId, IAdminService service) =>
        {
            var result = await service.DeleteCustomerAsync(customerId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Müşteri silinemedi.")
                };
            }
            return TypedResults.Ok();
        });

        // Veritabanını sıfırlama endpoint'i (SADECE DEVELOPMENT ORTAMINDA!)
        g.MapPost("/reset-database", async Task<Results<Ok<string>, BadRequest<string>, StatusCodeHttpResult>>
        (IWebHostEnvironment env, BankDbContext context) =>
        {
            // Sadece Development ortamında çalışsın
            if (!env.IsDevelopment())
            {
                return TypedResults.StatusCode(403);
            }

            try
            {
                // Tüm tabloları temizle
                await context.Database.ExecuteSqlRawAsync(@"
                    SET session_replication_role = 'replica';
                    TRUNCATE TABLE ""bank_transactions"" CASCADE;
                    TRUNCATE TABLE ""bank_transfers"" CASCADE;
                    TRUNCATE TABLE ""bank_cards"" CASCADE;
                    TRUNCATE TABLE ""bank_payment_orders"" CASCADE;
                    TRUNCATE TABLE ""bank_loans"" CASCADE;
                    TRUNCATE TABLE ""bank_credit_card_applications"" CASCADE;
                    TRUNCATE TABLE ""bank_accounts"" CASCADE;
                    TRUNCATE TABLE ""approval_workflows"" CASCADE;
                    TRUNCATE TABLE ""transaction_limits"" CASCADE;
                    TRUNCATE TABLE ""commissions"" CASCADE;
                    TRUNCATE TABLE ""kyc_verifications"" CASCADE;
                    TRUNCATE TABLE ""bill_payments"" CASCADE;
                    TRUNCATE TABLE ""notifications"" CASCADE;
                    TRUNCATE TABLE ""notification_preferences"" CASCADE;
                    TRUNCATE TABLE ""exchange_rates"" CASCADE;
                    TRUNCATE TABLE ""audit_logs"" CASCADE;
                    TRUNCATE TABLE ""password_reset_tokens"" CASCADE;
                    TRUNCATE TABLE ""bank_customers"" CASCADE;
                    TRUNCATE TABLE ""branches"" CASCADE;
                    TRUNCATE TABLE ""bill_institutions"" CASCADE;
                    SET session_replication_role = 'origin';
                ");

                return TypedResults.Ok("Veritabanı başarıyla sıfırlandı. Uygulama yeniden başlatıldığında admin kullanıcısı otomatik oluşturulacak.");
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest($"Veritabanı sıfırlama hatası: {ex.Message}");
            }
        });

        return app;
    }
}

