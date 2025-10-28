using System;
using NovaBank.Core.Enums;

namespace NovaBank.Core.ValueObjects
{
    /// <summary>Represents a monetary amount in a specific currency.</summary>
    public sealed class Money : IEquatable<Money>
    {
        private readonly decimal _amount;
        /// <summary>Amount value.</summary>
        public decimal Amount => _amount;
        /// <summary>Currency of the amount.</summary>
        public Currency Currency { get; }

        public Money(decimal amount, Currency currency)
        {
            _amount = amount;
            Currency = currency;
        }

        /// <summary>Returns a new Money that is the sum of two amounts. Enforces same currency.</summary>
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(_amount + other._amount, Currency);
        }

        /// <summary>Returns a new Money that is the difference of two amounts. Enforces same currency.</summary>
        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(_amount - other._amount, Currency);
        }

        /// <summary>Returns a new Money multiplied by a factor.</summary>
        public Money Multiply(decimal factor)
        {
            return new Money(_amount * factor, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (other is null) throw new ArgumentNullException(nameof(other));
            if (Currency != other.Currency)
                throw new ArgumentException("Currency mismatch.");
        }

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return _amount == other._amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(_amount, Currency);
        public override string ToString() => $"{_amount} {Currency}";
    }
}
