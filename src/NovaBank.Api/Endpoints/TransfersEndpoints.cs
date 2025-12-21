using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Transfers;
using NovaBank.Contracts.Transactions;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Endpoints;
public static class TransfersEndpoints
{
    public static IEndpointRouteBuilder MapTransfers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/transfers").RequireAuthorization("AnyUser");

        g.MapPost("/internal", async Task<Results<Ok<TransferResponse>, BadRequest<string>, NotFound, Conflict<string>>>
        (TransferInternalRequest req, ITransfersService service, CancellationToken ct) =>
        {
            var result = await service.TransferInternalAsync(req, ct);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.AccountNotFound => TypedResults.NotFound(),
                    ErrorCodes.SameAccountTransfer => TypedResults.BadRequest(result.ErrorMessage ?? "Aynı hesaba transfer yapılamaz."),
                    ErrorCodes.InvalidAmount => TypedResults.BadRequest(result.ErrorMessage ?? "Geçersiz tutar."),
                    ErrorCodes.CurrencyMismatch => TypedResults.Conflict(result.ErrorMessage ?? "Para birimi uyuşmuyor."),
                    ErrorCodes.InsufficientFunds => TypedResults.BadRequest(result.ErrorMessage ?? "Yetersiz bakiye."),
                    ErrorCodes.HesapDondurulmus => TypedResults.Conflict(result.ErrorMessage ?? "Hesap dondurulmuş."),
                    ErrorCodes.HesapKapali => TypedResults.Conflict(result.ErrorMessage ?? "Hesap kapalı."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Transfer işlemi başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        });

        g.MapPost("/external", async Task<Results<Ok<TransferResponse>, BadRequest<string>, NotFound, Conflict<string>>>
        (TransferExternalRequest req, ITransfersService service, CancellationToken ct) =>
        {
            var result = await service.TransferExternalAsync(req, ct);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.AccountNotFound => TypedResults.NotFound(),
                    ErrorCodes.InvalidAmount => TypedResults.BadRequest(result.ErrorMessage ?? "Geçersiz tutar."),
                    ErrorCodes.CurrencyMismatch => TypedResults.Conflict(result.ErrorMessage ?? "Para birimi uyuşmuyor."),
                    ErrorCodes.InsufficientFunds => TypedResults.BadRequest(result.ErrorMessage ?? "Yetersiz bakiye."),
                    ErrorCodes.HesapDondurulmus => TypedResults.Conflict(result.ErrorMessage ?? "Hesap dondurulmuş."),
                    ErrorCodes.HesapKapali => TypedResults.Conflict(result.ErrorMessage ?? "Hesap kapalı."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Transfer işlemi başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        });

        // Admin only: Transfer reversal
        var adminGroup = app.MapGroup("/api/v1/transfers").RequireAuthorization("AdminOnly");
        adminGroup.MapPost("/reverse", async Task<Results<Ok<ReverseTransferResponse>, BadRequest<string>, NotFound, Conflict<string>, StatusCodeHttpResult>>
            (ReverseTransferRequest req, ITransfersService service, CancellationToken ct) =>
        {
            var result = await service.ReverseTransferAsync(req, ct);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.NotFound => TypedResults.NotFound(),
                    ErrorCodes.Unauthorized => TypedResults.StatusCode(403),
                    ErrorCodes.AlreadyReversed => TypedResults.Conflict(result.ErrorMessage ?? "Transfer zaten iptal edilmiş."),
                    ErrorCodes.CannotReverseReversal => TypedResults.BadRequest(result.ErrorMessage ?? "Reversal transfer iptal edilemez."),
                    ErrorCodes.ReversalWindowExpired => TypedResults.BadRequest(result.ErrorMessage ?? "İptal süresi dolmuş."),
                    ErrorCodes.ExternalReversalNotSupported => TypedResults.BadRequest(result.ErrorMessage ?? "Dış transfer iptal edilemez."),
                    ErrorCodes.InsufficientFunds => TypedResults.BadRequest(result.ErrorMessage ?? "Yetersiz bakiye."),
                    ErrorCodes.AccountNotFound => TypedResults.NotFound(),
                    ErrorCodes.HesapKapali => TypedResults.Conflict(result.ErrorMessage ?? "Hesap kapalı."),
                    ErrorCodes.HesapDondurulmus => TypedResults.Conflict(result.ErrorMessage ?? "Hesap dondurulmuş."),
                    ErrorCodes.CurrencyMismatch => TypedResults.Conflict(result.ErrorMessage ?? "Para birimi uyuşmuyor."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Transfer iptal işlemi başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}
