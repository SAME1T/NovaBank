using NovaBank.Application.Common.Results;
using NovaBank.Contracts.CurrencyExchange;
using NovaBank.Core.Enums;

namespace NovaBank.Application.CurrencyExchange;

public interface ICurrencyExchangeService
{
    /// <summary>
    /// Döviz alım işlemi gerçekleştirir.
    /// TL hesabından çekip döviz hesabına yatırır.
    /// </summary>
    Task<Result<CurrencyExchangeResponse>> BuyCurrencyAsync(BuyCurrencyRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Döviz satım işlemi gerçekleştirir.
    /// Döviz hesabından çekip TL hesabına yatırır.
    /// </summary>
    Task<Result<CurrencyExchangeResponse>> SellCurrencyAsync(SellCurrencyRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Müşterinin döviz pozisyonlarını getirir.
    /// </summary>
    Task<Result<CurrencyPositionsResponse>> GetPositionsAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Belirli bir döviz için güncel kuru getirir.
    /// </summary>
    Task<Result<(decimal BuyRate, decimal SellRate, DateTime RateDate)>> GetCurrentRateAsync(Currency currency, CancellationToken ct = default);
    
    /// <summary>
    /// Döviz kurlarını veritabanına kaydeder.
    /// </summary>
    Task<Result<int>> SaveRatesAsync(SaveExchangeRatesRequest request, CancellationToken ct = default);
}
