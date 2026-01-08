using NovaBank.Core.Enums;

namespace NovaBank.Contracts.CurrencyExchange;

public sealed record BuyCurrencyRequest(
    Currency Currency,
    decimal Amount,
    Guid FromTryAccountId,
    Guid ToForeignAccountId,
    string? Description = null
);

public sealed record SellCurrencyRequest(
    Currency Currency,
    decimal Amount,
    Guid FromForeignAccountId,
    Guid ToTryAccountId,
    string? Description = null
);

public sealed record CurrencyExchangeResponse(
    Guid TransactionId,
    string ReferenceCode,
    string Currency,
    decimal Amount,
    decimal ExchangeRate,
    decimal TryAmount,
    decimal Commission,
    decimal NetTryAmount,
    decimal? RealizedPnlTry,
    decimal? RealizedPnlPercent,
    PositionSnapshot NewPosition
);

public sealed record PositionSnapshot(
    decimal TotalAmount,
    decimal AverageCostRate,
    decimal TotalCostTry
);

public sealed record CurrencyPositionResponse(
    string Currency,
    decimal TotalAmount,
    decimal AverageCostRate,
    decimal TotalCostTry,
    decimal CurrentRate,
    decimal CurrentValue,
    decimal UnrealizedPnlTry,
    decimal UnrealizedPnlPercent
);

public sealed record CurrencyPositionsResponse(
    List<CurrencyPositionResponse> Positions,
    decimal TotalCostTry,
    decimal TotalCurrentValue,
    decimal TotalUnrealizedPnlTry,
    decimal TotalUnrealizedPnlPercent
);

/// <summary>
/// Döviz kurlarını kaydetmek için istek.
/// </summary>
public sealed record SaveExchangeRatesRequest(
    DateTime RateDate,
    List<ExchangeRateItem> Rates
);

public sealed record ExchangeRateItem(
    string CurrencyCode,
    decimal BuyRate,
    decimal SellRate
);
