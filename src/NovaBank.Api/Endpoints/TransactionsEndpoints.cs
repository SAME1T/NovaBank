using Microsoft.AspNetCore.Http.HttpResults;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Transactions;
using NovaBank.Contracts.Transactions;

namespace NovaBank.Api.Endpoints;
public static class TransactionsEndpoints
{
    public static IEndpointRouteBuilder MapTransactions(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/transactions").RequireAuthorization("AnyUser");

        g.MapPost("/deposit", async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound, Conflict<string>>>
        (DepositRequest req, ITransactionsService service, CancellationToken ct) =>
        {
            var result = await service.DepositAsync(req, ct);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.AccountNotFound => TypedResults.NotFound(),
                    ErrorCodes.CurrencyMismatch => TypedResults.Conflict(result.ErrorMessage ?? "Para birimi uyuşmuyor."),
                    ErrorCodes.InvalidAmount => TypedResults.BadRequest(result.ErrorMessage ?? "Geçersiz tutar."),
                    ErrorCodes.HesapDondurulmus => TypedResults.Conflict(result.ErrorMessage ?? "Hesap dondurulmuş."),
                    ErrorCodes.HesapKapali => TypedResults.Conflict(result.ErrorMessage ?? "Hesap kapalı."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Para yatırma işlemi başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        });

        g.MapPost("/withdraw", async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound, Conflict<string>>>
        (WithdrawRequest req, ITransactionsService service, CancellationToken ct) =>
        {
            var result = await service.WithdrawAsync(req, ct);
            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCodes.AccountNotFound => TypedResults.NotFound(),
                    ErrorCodes.InsufficientFunds => TypedResults.BadRequest(result.ErrorMessage ?? "Yetersiz bakiye."),
                    ErrorCodes.CurrencyMismatch => TypedResults.Conflict(result.ErrorMessage ?? "Para birimi uyuşmuyor."),
                    ErrorCodes.InvalidAmount => TypedResults.BadRequest(result.ErrorMessage ?? "Geçersiz tutar."),
                    ErrorCodes.HesapDondurulmus => TypedResults.Conflict(result.ErrorMessage ?? "Hesap dondurulmuş."),
                    ErrorCodes.HesapKapali => TypedResults.Conflict(result.ErrorMessage ?? "Hesap kapalı."),
                    _ => TypedResults.BadRequest(result.ErrorMessage ?? "Para çekme işlemi başarısız.")
                };
            }

            return TypedResults.Ok(result.Value!);
        });

        return app;
    }
}
