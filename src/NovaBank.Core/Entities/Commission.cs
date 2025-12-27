using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Komisyon tanımı.
/// </summary>
public sealed class Commission : Entity
{
    private Commission() { }

    public CommissionType CommissionType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Currency Currency { get; private set; }
    public decimal FixedAmount { get; private set; }
    public decimal PercentageRate { get; private set; } // 0.00125 = %0.125
    public decimal MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }

    public Commission(
        CommissionType commissionType,
        string name,
        Currency currency,
        decimal fixedAmount = 0,
        decimal percentageRate = 0,
        decimal minAmount = 0,
        decimal? maxAmount = null,
        string? description = null,
        DateTime? validFrom = null,
        DateTime? validUntil = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Komisyon adı gerekli.", nameof(name));
        if (fixedAmount < 0)
            throw new ArgumentException("Sabit tutar negatif olamaz.", nameof(fixedAmount));
        if (percentageRate < 0)
            throw new ArgumentException("Yüzde oranı negatif olamaz.", nameof(percentageRate));

        CommissionType = commissionType;
        Name = name.Trim();
        Description = description?.Trim();
        Currency = currency;
        FixedAmount = fixedAmount;
        PercentageRate = percentageRate;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        ValidFrom = validFrom ?? DateTime.UtcNow.Date;
        ValidUntil = validUntil;
    }

    public void Update(
        string name,
        decimal fixedAmount,
        decimal percentageRate,
        decimal minAmount,
        decimal? maxAmount,
        string? description,
        DateTime? validUntil)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Komisyon adı gerekli.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        FixedAmount = fixedAmount;
        PercentageRate = percentageRate;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        ValidUntil = validUntil;
        TouchUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        TouchUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        TouchUpdated();
    }

    /// <summary>
    /// Verilen işlem tutarı için komisyon hesaplar.
    /// </summary>
    public decimal Calculate(decimal transactionAmount)
    {
        var commission = FixedAmount + (transactionAmount * PercentageRate);
        
        if (commission < MinAmount)
            commission = MinAmount;
        
        if (MaxAmount.HasValue && commission > MaxAmount.Value)
            commission = MaxAmount.Value;
        
        return Math.Round(commission, 2);
    }

    /// <summary>
    /// Komisyonun belirtilen tarihte geçerli olup olmadığını kontrol eder.
    /// </summary>
    public bool IsValidAt(DateTime date)
    {
        if (!IsActive) return false;
        if (date < ValidFrom) return false;
        if (ValidUntil.HasValue && date > ValidUntil.Value) return false;
        return true;
    }
}
