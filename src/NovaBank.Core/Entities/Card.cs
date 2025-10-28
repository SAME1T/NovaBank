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
        public CardType CardType { get; private set; }
        public CardStatus CardStatus { get; private set; }
        public string MaskedPan { get; private set; }
        public int ExpiryMonth { get; private set; }
        public int ExpiryYear { get; private set; }
        public decimal? CreditLimit { get; private set; }
        public decimal? AvailableLimit { get; private set; }

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
    }
}
