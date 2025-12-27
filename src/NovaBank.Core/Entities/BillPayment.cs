using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Fatura ödeme kaydı.
/// </summary>
public sealed class BillPayment : Entity
{
    private BillPayment() { }

    public Guid? AccountId { get; private set; }
    public Guid? CardId { get; private set; }
    public Guid InstitutionId { get; private set; }
    public string SubscriberNo { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal Commission { get; private set; }
    public string? ReferenceCode { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime PaidAt { get; private set; }
    public PaymentStatus Status { get; private set; }

    public BillPayment(
        Guid? accountId,
        Guid? cardId,
        Guid institutionId,
        string subscriberNo,
        decimal amount,
        decimal commission = 0,
        DateTime? dueDate = null,
        string? referenceCode = null)
    {
        if (string.IsNullOrWhiteSpace(subscriberNo))
            throw new ArgumentException("Abone numarası gerekli.", nameof(subscriberNo));
        if (amount <= 0)
            throw new ArgumentException("Tutar 0'dan büyük olmalı.", nameof(amount));
        if (accountId == null && cardId == null)
            throw new ArgumentException("Hesap veya Kart ID'si gerekli.");

        AccountId = accountId;
        CardId = cardId;
        InstitutionId = institutionId;
        SubscriberNo = subscriberNo.Trim();
        Amount = amount;
        Commission = commission;
        DueDate = dueDate;
        ReferenceCode = referenceCode ?? Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        PaidAt = DateTime.UtcNow;
        Status = PaymentStatus.Executed;
    }

    public void MarkFailed()
    {
        Status = PaymentStatus.Failed;
        TouchUpdated();
    }

    /// <summary>
    /// Toplam tutar (fatura + komisyon).
    /// </summary>
    public decimal TotalAmount => Amount + Commission;
}
