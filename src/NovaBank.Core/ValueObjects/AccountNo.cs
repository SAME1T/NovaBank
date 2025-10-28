using System;

namespace NovaBank.Core.ValueObjects
{
    /// <summary>Represents a bank account number.</summary>
    public sealed class AccountNo : IEquatable<AccountNo>
    {
        private readonly long _value;
        public long Value => _value;

        public AccountNo(long value)
        {
            if (value <= 0) throw new ArgumentException("Account number must be positive.", nameof(value));
            _value = value;
        }

        public bool Equals(AccountNo? other) => other is not null && _value == other._value;
        public override bool Equals(object? obj) => Equals(obj as AccountNo);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString();
    }
}
