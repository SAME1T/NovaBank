using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// İşlem limiti tanımı.
/// </summary>
public sealed class TransactionLimit : Entity
{
    private TransactionLimit() { }

    public LimitType LimitType { get; private set; }
    public LimitScope Scope { get; private set; }
    public Guid? ScopeId { get; private set; } // Customer veya Account ID (global için null)
    public UserRole? ScopeRole { get; private set; } // Role scope için
    public Currency Currency { get; private set; }
    public decimal MinAmount { get; private set; }
    public decimal MaxAmount { get; private set; }
    public decimal? RequiresApprovalAbove { get; private set; }
    public bool IsActive { get; private set; } = true;

    public TransactionLimit(
        LimitType limitType,
        LimitScope scope,
        Currency currency,
        decimal maxAmount,
        decimal minAmount = 0,
        Guid? scopeId = null,
        UserRole? scopeRole = null,
        decimal? requiresApprovalAbove = null)
    {
        if (maxAmount <= 0)
            throw new ArgumentException("Maksimum tutar 0'dan büyük olmalı.", nameof(maxAmount));
        if (minAmount < 0)
            throw new ArgumentException("Minimum tutar negatif olamaz.", nameof(minAmount));
        if (minAmount > maxAmount)
            throw new ArgumentException("Minimum tutar, maksimum tutardan büyük olamaz.");

        LimitType = limitType;
        Scope = scope;
        ScopeId = scopeId;
        ScopeRole = scopeRole;
        Currency = currency;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        RequiresApprovalAbove = requiresApprovalAbove;
    }

    public void Update(decimal maxAmount, decimal minAmount, decimal? requiresApprovalAbove)
    {
        if (maxAmount <= 0)
            throw new ArgumentException("Maksimum tutar 0'dan büyük olmalı.", nameof(maxAmount));
        if (minAmount < 0)
            throw new ArgumentException("Minimum tutar negatif olamaz.", nameof(minAmount));

        MaxAmount = maxAmount;
        MinAmount = minAmount;
        RequiresApprovalAbove = requiresApprovalAbove;
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
    /// Verilen tutarın limite uygun olup olmadığını kontrol eder.
    /// </summary>
    public bool IsWithinLimit(decimal amount)
    {
        return amount >= MinAmount && amount <= MaxAmount;
    }

    /// <summary>
    /// Verilen tutarın onay gerektirip gerektirmediğini kontrol eder.
    /// </summary>
    public bool RequiresApproval(decimal amount)
    {
        return RequiresApprovalAbove.HasValue && amount > RequiresApprovalAbove.Value;
    }
}
