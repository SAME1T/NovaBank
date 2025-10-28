using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents an account transaction.</summary>
    public sealed class Transaction : Entity
    {
        private Transaction() { }
        public Guid AccountId { get; private set; }
        public Money Amount { get; private set; }
        public TransactionDirection Direction { get; private set; }
        public string Description { get; private set; }
        public string ReferenceCode { get; private set; }

        public Transaction(Guid accountId, Money amount, TransactionDirection direction, string description)
        {
            if (amount is null) throw new ArgumentNullException(nameof(amount));
            if (amount.Amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            AccountId = accountId;
            Amount = amount;
            Direction = direction;
            Description = description ?? string.Empty;
            ReferenceCode = Guid.NewGuid().ToString("N");
        }
    }
}
