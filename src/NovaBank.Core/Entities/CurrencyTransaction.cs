using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Döviz alım/satım işlem kaydı.
/// </summary>
public sealed class CurrencyTransaction : Entity
{
    private CurrencyTransaction() { }

    public Guid CustomerId { get; private set; }
    
    /// <summary>İşlem tipi: BUY veya SELL</summary>
    public string TransactionType { get; private set; } = string.Empty;
    
    public Currency Currency { get; private set; }
    
    /// <summary>Döviz miktarı</summary>
    public decimal Amount { get; private set; }
    
    /// <summary>İşlem anındaki kur</summary>
    public decimal ExchangeRate { get; private set; }
    
    /// <summary>Kur tipi: BUY (banka alış) veya SELL (banka satış)</summary>
    public string RateType { get; private set; } = string.Empty;
    
    public string RateSource { get; private set; } = "TCMB";
    public DateTime RateDate { get; private set; }
    
    /// <summary>Brüt TL tutarı</summary>
    public decimal TryAmount { get; private set; }
    
    /// <summary>Komisyon (TL)</summary>
    public decimal CommissionTry { get; private set; }
    
    /// <summary>Net TL tutarı</summary>
    public decimal NetTryAmount { get; private set; }
    
    public Guid FromAccountId { get; private set; }
    public Guid ToAccountId { get; private set; }
    
    // Pozisyon snapshot
    public decimal? PositionBeforeAmount { get; private set; }
    public decimal? PositionAfterAmount { get; private set; }
    public decimal? AvgCostBefore { get; private set; }
    public decimal? AvgCostAfter { get; private set; }
    
    // Kâr/Zarar (sadece SELL için)
    public decimal? RealizedPnlTry { get; private set; }
    public decimal? RealizedPnlPercent { get; private set; }
    
    public string? Description { get; private set; }
    public string ReferenceCode { get; private set; } = string.Empty;

    /// <summary>
    /// Döviz ALIM işlemi oluşturur.
    /// </summary>
    public static CurrencyTransaction CreateBuy(
        Guid customerId,
        Currency currency,
        decimal amount,
        decimal exchangeRate,
        DateTime rateDate,
        decimal tryAmount,
        decimal commission,
        Guid fromTryAccountId,
        Guid toForeignAccountId,
        decimal positionBefore,
        decimal positionAfter,
        decimal avgCostBefore,
        decimal avgCostAfter,
        string? description = null)
    {
        return new CurrencyTransaction
        {
            CustomerId = customerId,
            TransactionType = "BUY",
            Currency = currency,
            Amount = amount,
            ExchangeRate = exchangeRate,
            RateType = "SELL", // Banka satış kuru
            RateSource = "TCMB",
            RateDate = rateDate,
            TryAmount = tryAmount,
            CommissionTry = commission,
            NetTryAmount = tryAmount + commission, // Alımda toplam ödenen
            FromAccountId = fromTryAccountId,
            ToAccountId = toForeignAccountId,
            PositionBeforeAmount = positionBefore,
            PositionAfterAmount = positionAfter,
            AvgCostBefore = avgCostBefore,
            AvgCostAfter = avgCostAfter,
            RealizedPnlTry = null,
            RealizedPnlPercent = null,
            Description = description ?? "Döviz alımı",
            ReferenceCode = GenerateReferenceCode()
        };
    }

    /// <summary>
    /// Döviz SATIM işlemi oluşturur.
    /// </summary>
    public static CurrencyTransaction CreateSell(
        Guid customerId,
        Currency currency,
        decimal amount,
        decimal exchangeRate,
        DateTime rateDate,
        decimal tryAmount,
        decimal commission,
        Guid fromForeignAccountId,
        Guid toTryAccountId,
        decimal positionBefore,
        decimal positionAfter,
        decimal avgCostBefore,
        decimal avgCostAfter,
        decimal realizedPnlTry,
        decimal realizedPnlPercent,
        string? description = null)
    {
        return new CurrencyTransaction
        {
            CustomerId = customerId,
            TransactionType = "SELL",
            Currency = currency,
            Amount = amount,
            ExchangeRate = exchangeRate,
            RateType = "BUY", // Banka alış kuru
            RateSource = "TCMB",
            RateDate = rateDate,
            TryAmount = tryAmount,
            CommissionTry = commission,
            NetTryAmount = tryAmount - commission, // Satımda net alınan
            FromAccountId = fromForeignAccountId,
            ToAccountId = toTryAccountId,
            PositionBeforeAmount = positionBefore,
            PositionAfterAmount = positionAfter,
            AvgCostBefore = avgCostBefore,
            AvgCostAfter = avgCostAfter,
            RealizedPnlTry = realizedPnlTry,
            RealizedPnlPercent = realizedPnlPercent,
            Description = description ?? "Döviz satımı",
            ReferenceCode = GenerateReferenceCode()
        };
    }

    private static string GenerateReferenceCode()
    {
        return $"FX{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }
}
