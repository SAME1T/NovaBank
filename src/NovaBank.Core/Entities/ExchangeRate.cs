using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Döviz kuru kaydı.
/// </summary>
public sealed class ExchangeRate : Entity
{
    private ExchangeRate() { }

    public Currency BaseCurrency { get; private set; }
    public Currency TargetCurrency { get; private set; }
    public decimal BuyRate { get; private set; }
    public decimal SellRate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string Source { get; private set; } = "TCMB";

    public ExchangeRate(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal buyRate,
        decimal sellRate,
        DateTime effectiveDate,
        string source = "TCMB")
    {
        if (buyRate <= 0)
            throw new ArgumentException("Alış kuru 0'dan büyük olmalı.", nameof(buyRate));
        if (sellRate <= 0)
            throw new ArgumentException("Satış kuru 0'dan büyük olmalı.", nameof(sellRate));
        if (buyRate > sellRate)
            throw new ArgumentException("Alış kuru, satış kurundan büyük olamaz.");

        BaseCurrency = baseCurrency;
        TargetCurrency = targetCurrency;
        BuyRate = buyRate;
        SellRate = sellRate;
        // PostgreSQL timestamp with time zone için UTC gerekli
        EffectiveDate = DateTime.SpecifyKind(effectiveDate.Date, DateTimeKind.Utc);
        Source = source?.Trim() ?? "TCMB";
    }

    public void Update(decimal buyRate, decimal sellRate)
    {
        if (buyRate <= 0)
            throw new ArgumentException("Alış kuru 0'dan büyük olmalı.", nameof(buyRate));
        if (sellRate <= 0)
            throw new ArgumentException("Satış kuru 0'dan büyük olmalı.", nameof(sellRate));

        BuyRate = buyRate;
        SellRate = sellRate;
        TouchUpdated();
    }

    /// <summary>
    /// Döviz alım hesaplama (müşteri döviz alıyor = banka satıyor).
    /// </summary>
    public decimal CalculateBuy(decimal foreignAmount)
    {
        return Math.Round(foreignAmount * SellRate, 2);
    }

    /// <summary>
    /// Döviz satış hesaplama (müşteri döviz satıyor = banka alıyor).
    /// </summary>
    public decimal CalculateSell(decimal foreignAmount)
    {
        return Math.Round(foreignAmount * BuyRate, 2);
    }

    /// <summary>
    /// Spread (alış-satış farkı) yüzdesi.
    /// </summary>
    public decimal SpreadPercentage => Math.Round((SellRate - BuyRate) / BuyRate * 100, 4);
}
