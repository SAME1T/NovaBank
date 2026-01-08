using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.CreditCards;
using NovaBank.Contracts.CreditCards;

namespace NovaBank.Api.Endpoints;

public static class CreditCardEndpoints
{
    public static IEndpointRouteBuilder MapCreditCards(this IEndpointRouteBuilder app)
    {
        // Müşteri işlemleri
        var customer = app.MapGroup("/api/v1/credit-cards").RequireAuthorization();

        customer.MapPost("/apply", async Task<Results<Ok, BadRequest<string>, UnauthorizedHttpResult>>
        (ApplyCreditCardRequest req, ICreditCardService service) =>
        {
            var result = await service.ApplyForCreditCardAsync(req);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Başvuru yapılamadı.")
                };
            }
            return TypedResults.Ok();
        });

        customer.MapGet("/my-cards", async Task<Results<Ok<List<CreditCardSummaryResponse>>, BadRequest<string>, UnauthorizedHttpResult>>
        (ICreditCardService service) =>
        {
            var result = await service.GetMyCardsAsync();
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Kartlar alınamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        customer.MapGet("/my-applications", async Task<Results<Ok<List<MyApplicationResponse>>, BadRequest<string>, UnauthorizedHttpResult>>
        (ICreditCardService service) =>
        {
            var result = await service.GetMyApplicationsAsync();
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Başvurular alınamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        customer.MapPost("/{cardId:guid}/payment", async Task<Results<Ok, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid cardId, CardPaymentRequest req, ICreditCardService service) =>
        {
            var result = await service.MakeCardPaymentAsync(cardId, req.Amount, req.FromAccountId);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Ödeme yapılamadı.")
                };
            }
            return TypedResults.Ok();
        });

        // Admin/BranchManager işlemleri (Kredi kartı başvurularını yönetme)
        var admin = app.MapGroup("/api/v1/admin/credit-card-applications").RequireAuthorization("AdminOrBranchManager");

        admin.MapGet("/", async Task<Results<Ok<List<CreditCardApplicationResponse>>, BadRequest<string>, UnauthorizedHttpResult>>
        (ICreditCardService service) =>
        {
            var result = await service.GetPendingApplicationsAsync();
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Başvurular alınamadı.")
                };
            }
            return TypedResults.Ok(result.Value!);
        });

        admin.MapPost("/{applicationId:guid}/approve", async Task<Results<Ok, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid applicationId, ApproveCreditCardRequest req, ICreditCardService service) =>
        {
            var result = await service.ApproveApplicationAsync(applicationId, req.ApprovedLimit);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Başvuru onaylanamadı.")
                };
            }
            return TypedResults.Ok();
        });

        admin.MapPost("/{applicationId:guid}/reject", async Task<Results<Ok, BadRequest<string>, NotFound, UnauthorizedHttpResult>>
        (Guid applicationId, RejectCreditCardRequest req, ICreditCardService service) =>
        {
            var result = await service.RejectApplicationAsync(applicationId, req.Reason);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Başvuru reddedilemedi.")
                };
            }
            return TypedResults.Ok();
        });

        admin.MapPost("/process-interests", async Task<Results<Ok, BadRequest<string>, UnauthorizedHttpResult>>
        (ICreditCardService service) =>
        {
            var result = await service.ProcessInterestsAsync();
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.Unauthorized => TypedResults.Unauthorized(),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "İşlem yapılamadı.")
                };
            }
            return TypedResults.Ok();
        });

        return app;
    }
}
