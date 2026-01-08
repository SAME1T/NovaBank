using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Müşterinin döviz pozisyonunu temsil eder.
/// Ortalama maliyet yöntemi ile portföy takibi yapılır.
/// </summary>
public sealed class CurrencyPosition : Entity
{
    private CurrencyPosition() { }

    public Guid CustomerId { get; private set; }
    public Currency Currency { get; private set; }
    
    /// <summary>Toplam döviz miktarı</summary>
    public decimal TotalAmount { get; private set; }
    
    /// <summary>Ağırlıklı ortalama maliyet kuru (TL/Döviz)</summary>
    public decimal AverageCostRate { get; private set; }
    
    /// <summary>Toplam TL maliyeti</summary>
    public decimal TotalCostTry { get; private set; }

    public CurrencyPosition(Guid customerId, Currency currency)
    {
        if (currency == Currency.TRY)
            throw new ArgumentException("TL için pozisyon oluşturulamaz.", nameof(currency));
            
        CustomerId = customerId;
        Currency = currency;
        TotalAmount = 0;
        AverageCostRate = 0;
        TotalCostTry = 0;
    }

    /// <summary>
    /// Döviz alımı sonrası pozisyonu günceller.
    /// Ağırlıklı ortalama maliyet hesaplar.
    /// </summary>
    /// <param name="amount">Alınan döviz miktarı</param>
    /// <param name="costTry">Ödenen TL tutarı (komisyon dahil)</param>
    public void AddPosition(decimal amount, decimal costTry)
    {
        if (amount <= 0)
            throw new ArgumentException("Miktar pozitif olmalı.", nameof(amount));
        if (costTry <= 0)
            throw new ArgumentException("Maliyet pozitif olmalı.", nameof(costTry));

        var newTotalAmount = TotalAmount + amount;
        var newTotalCost = TotalCostTry + costTry;
        
        TotalAmount = newTotalAmount;
        TotalCostTry = newTotalCost;
        AverageCostRate = newTotalCost / newTotalAmount;
        
        TouchUpdated();
    }

    /// <summary>
    /// Döviz satışı sonrası pozisyonu günceller.
    /// </summary>
    /// <param name="amount">Satılan döviz miktarı</param>
    /// <returns>Satılan miktarın TL maliyeti (kâr/zarar hesaplaması için)</returns>
    public decimal RemovePosition(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Miktar pozitif olmalı.", nameof(amount));
        if (amount > TotalAmount)
            throw new InvalidOperationException("Satılacak miktar mevcut pozisyondan fazla olamaz.");

        // Satılan miktarın maliyeti = miktar × ortalama maliyet
        var soldCost = amount * AverageCostRate;
        
        TotalAmount -= amount;
        TotalCostTry -= soldCost;
        
        // Eğer pozisyon sıfırlandıysa ortalama maliyeti de sıfırla
        if (TotalAmount <= 0)
        {
            TotalAmount = 0;
            TotalCostTry = 0;
            AverageCostRate = 0;
        }
        
        TouchUpdated();
        
        return Math.Round(soldCost, 2);
    }

    /// <summary>
    /// Güncel kur ile pozisyonun TL değerini hesaplar.
    /// </summary>
    /// <param name="currentBuyRate">Güncel alış kuru (banka alış = müşteri satış)</param>
    public decimal CalculateCurrentValue(decimal currentBuyRate)
    {
        return Math.Round(TotalAmount * currentBuyRate, 2);
    }

    /// <summary>
    /// Gerçekleşmemiş kâr/zarar hesaplar.
    /// </summary>
    /// <param name="currentBuyRate">Güncel alış kuru</param>
    /// <returns>(Kâr/Zarar TL, Kâr/Zarar %)</returns>
    public (decimal PnlTry, decimal PnlPercent) CalculateUnrealizedPnl(decimal currentBuyRate)
    {
        if (TotalAmount <= 0 || TotalCostTry <= 0)
            return (0, 0);

        var currentValue = CalculateCurrentValue(currentBuyRate);
        var pnlTry = currentValue - TotalCostTry;
        var pnlPercent = (pnlTry / TotalCostTry) * 100;
        
        return (Math.Round(pnlTry, 2), Math.Round(pnlPercent, 2));
    }
}
