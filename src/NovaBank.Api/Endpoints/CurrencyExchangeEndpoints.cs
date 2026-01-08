using Microsoft.AspNetCore.Mvc;
using NovaBank.Application.CurrencyExchange;
using NovaBank.Contracts.CurrencyExchange;
using NovaBank.Core.Enums;

namespace NovaBank.Api.Endpoints;

public static class CurrencyExchangeEndpoints
{
    public static void MapCurrencyExchangeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/currency-exchange")
            .WithTags("Currency Exchange")
            .RequireAuthorization();

        group.MapPost("/buy", BuyCurrencyAsync)
            .WithName("BuyCurrency")
            .WithSummary("Döviz alım işlemi gerçekleştirir");

        group.MapPost("/sell", SellCurrencyAsync)
            .WithName("SellCurrency")
            .WithSummary("Döviz satım işlemi gerçekleştirir");

        group.MapGet("/positions", GetPositionsAsync)
            .WithName("GetCurrencyPositions")
            .WithSummary("Müşterinin döviz pozisyonlarını getirir");

        group.MapGet("/rate/{currency}", GetCurrentRateAsync)
            .WithName("GetCurrentRate")
            .WithSummary("Belirli bir döviz için güncel kuru getirir");

        group.MapPost("/rates", SaveRatesAsync)
            .WithName("SaveExchangeRates")
            .WithSummary("Döviz kurlarını veritabanına kaydeder");
    }

    private static async Task<IResult> BuyCurrencyAsync(
        [FromBody] BuyCurrencyRequest request,
        ICurrencyExchangeService service,
        CancellationToken ct)
    {
        var result = await service.BuyCurrencyAsync(request, ct);
        
        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
            
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SellCurrencyAsync(
        [FromBody] SellCurrencyRequest request,
        ICurrencyExchangeService service,
        CancellationToken ct)
    {
        var result = await service.SellCurrencyAsync(request, ct);
        
        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
            
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetPositionsAsync(
        ICurrencyExchangeService service,
        CancellationToken ct)
    {
        var result = await service.GetPositionsAsync(ct);
        
        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
            
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetCurrentRateAsync(
        [FromRoute] Currency currency,
        ICurrencyExchangeService service,
        CancellationToken ct)
    {
        var result = await service.GetCurrentRateAsync(currency, ct);
        
        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
            
        var (buyRate, sellRate, rateDate) = result.Value;
        return Results.Ok(new { buyRate, sellRate, rateDate });
    }

    private static async Task<IResult> SaveRatesAsync(
        [FromBody] SaveExchangeRatesRequest request,
        ICurrencyExchangeService service,
        CancellationToken ct)
    {
        var result = await service.SaveRatesAsync(request, ct);
        
        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
            
        return Results.Ok(new { savedCount = result.Value, message = $"{result.Value} kur kaydedildi." });
    }
}
