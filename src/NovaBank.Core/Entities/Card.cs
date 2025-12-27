using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents a payment card.</summary>
    public sealed class Card : Entity
    {
        private Card() { }
        public Guid AccountId { get; private set; }
        public Guid? CustomerId { get; private set; }
        public CardType CardType { get; private set; }
        public CardStatus CardStatus { get; private set; }
        public string MaskedPan { get; private set; }
        public int ExpiryMonth { get; private set; }
        public int ExpiryYear { get; private set; }
        public decimal? CreditLimit { get; private set; }
        public decimal? AvailableLimit { get; private set; }
        public decimal CurrentDebt { get; private set; } = 0;
        public DateTime? MinPaymentDueDate { get; private set; }
        public decimal? MinPaymentAmount { get; private set; }
        public int BillingCycleDay { get; private set; } = 1;
        public bool IsApproved { get; private set; } = false;

        public Card(Guid accountId, CardType cardType, string maskedPan, int expiryMonth, int expiryYear, decimal? creditLimit = null, decimal? availableLimit = null)
        {
            if (string.IsNullOrWhiteSpace(maskedPan)) throw new ArgumentException("Masked PAN is required.", nameof(maskedPan));
            if (expiryMonth < 1 || expiryMonth > 12) throw new ArgumentException("Invalid expiry month.", nameof(expiryMonth));
            AccountId = accountId;
            CardType = cardType;
            CardStatus = CardStatus.Active;
            MaskedPan = maskedPan.Trim();
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            CreditLimit = creditLimit;
            AvailableLimit = availableLimit;
            IsApproved = cardType == CardType.Debit; // Debit kartlar otomatik onaylı
        }

        /// <summary>Kredi kartı oluştur (başvuru onaylandığında).</summary>
        public static Card CreateCreditCard(Guid customerId, Guid accountId, decimal creditLimit, int billingCycleDay = 1)
        {
            var card = new Card
            {
                CustomerId = customerId,
                AccountId = accountId,
                CardType = CardType.Credit,
                CardStatus = CardStatus.Active,
                MaskedPan = GenerateMaskedPan(),
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.Year + 5,
                CreditLimit = creditLimit,
                AvailableLimit = creditLimit,
                CurrentDebt = 0,
                BillingCycleDay = Math.Clamp(billingCycleDay, 1, 28),
                IsApproved = true
            };
            card.CalculateNextPaymentDue();
            return card;
        }

        private static string GenerateMaskedPan()
        {
            var random = new Random();
            var last4 = random.Next(1000, 9999);
            return $"**** **** **** {last4}";
        }

        /// <summary>Blocks the card.</summary>
        public void Block()
        {
            if (CardStatus != CardStatus.Closed)
                CardStatus = CardStatus.Blocked;
            TouchUpdated();
        }

        /// <summary>Unblocks the card.</summary>
        public void Unblock()
        {
            if (CardStatus == CardStatus.Blocked)
                CardStatus = CardStatus.Active;
            TouchUpdated();
        }

        /// <summary>Checks whether the card can authorize the given amount.</summary>
        public bool CanAuthorize(Money amount)
        {
            if (amount is null) return false;
            if (CardType == CardType.Credit)
            {
                if (AvailableLimit is null) return false;
                return AvailableLimit.Value >= amount.Amount;
            }
            return amount.Amount >= 0;
        }

        /// <summary>Validates authorization without mutating limits or balances.</summary>
        public void Authorize(Money amount)
        {
            if (!CanAuthorize(amount)) throw new InvalidOperationException("Authorization denied.");
        }

        /// <summary>Harcama ekle (borcu artır).</summary>
        public void AddSpending(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Harcama tutarı pozitif olmalı.", nameof(amount));
            if (CardType != CardType.Credit) throw new InvalidOperationException("Sadece kredi kartlarında harcama yapılabilir.");
            if (AvailableLimit < amount) throw new InvalidOperationException("Yetersiz limit.");

            CurrentDebt += amount;
            AvailableLimit -= amount;
            CalculateMinPayment();
            TouchUpdated();
        }

        /// <summary>Ödeme yap (borcu azalt).</summary>
        public void MakePayment(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Ödeme tutarı pozitif olmalı.", nameof(amount));
            if (amount > CurrentDebt) amount = CurrentDebt; // Fazla ödeme yapılmasın

            CurrentDebt -= amount;
            AvailableLimit = (CreditLimit ?? 0) - CurrentDebt;
            CalculateMinPayment();
            TouchUpdated();
        }

        /// <summary>Minimum ödeme tutarını hesapla.</summary>
        private void CalculateMinPayment()
        {
            if (CurrentDebt <= 0)
            {
                MinPaymentAmount = 0;
                MinPaymentDueDate = null;
            }
            else
            {
                // Minimum ödeme: Borcun %10'u veya 100 TL (hangisi büyükse)
                MinPaymentAmount = Math.Max(CurrentDebt * 0.10m, Math.Min(CurrentDebt, 100m));
                CalculateNextPaymentDue();
            }
        }

        /// <summary>Bir sonraki ödeme tarihini hesapla.</summary>
        private void CalculateNextPaymentDue()
        {
            var today = DateTime.UtcNow.Date;
            var dueDay = BillingCycleDay + 10; // Ekstre kesiminden 10 gün sonra
            if (dueDay > 28) dueDay = 28;

            var dueDate = new DateTime(today.Year, today.Month, dueDay, 0, 0, 0, DateTimeKind.Utc);
            if (dueDate <= today)
                dueDate = dueDate.AddMonths(1);

            MinPaymentDueDate = dueDate;
        }

        /// <summary>Limit güncelle (admin tarafından).</summary>
        public void UpdateCreditLimit(decimal newLimit)
        {
            if (CardType != CardType.Credit) throw new InvalidOperationException("Sadece kredi kartı limiti güncellenebilir.");
            if (newLimit < CurrentDebt) throw new InvalidOperationException("Yeni limit mevcut borçtan düşük olamaz.");

            var difference = newLimit - (CreditLimit ?? 0);
            CreditLimit = newLimit;
            AvailableLimit = (AvailableLimit ?? 0) + difference;
            TouchUpdated();
        }

        /// <summary>Gecikme faizi uygula (%5 faiz oranı).</summary>
        public void ApplyInterest(decimal interestRate = 0.05m)
        {
            if (CardType != CardType.Credit || CurrentDebt <= 0) return;
            
            // Eğer son ödeme tarihi geçmişse faiz uygula
            if (MinPaymentDueDate.HasValue && DateTime.UtcNow > MinPaymentDueDate.Value)
            {
                var interestAmount = Math.Round(CurrentDebt * interestRate, 2);
                if (interestAmount > 0)
                {
                    CurrentDebt += interestAmount;
                    AvailableLimit = (CreditLimit ?? 0) - CurrentDebt;
                    CalculateMinPayment();
                    TouchUpdated();
                }
            }
        }
    }
}
