using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Transfers;
using NovaBank.Contracts.Transactions;

namespace NovaBank.Api.Endpoints;
public static class TransfersEndpoints
{
    public static IEndpointRouteBuilder MapTransfers(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/transfers");

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
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Transfer işlemi başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}
