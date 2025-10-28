using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NovaBank.Core.ValueObjects
{
    /// <summary>International Bank Account Number with minimal validation.</summary>
    public sealed class Iban : IEquatable<Iban>
    {
        private readonly string _value;
        public string Value => _value;

        public Iban(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", nameof(value));
            var v = Regex.Replace(value.ToUpperInvariant(), "\\s+", string.Empty);
            if (v.Length < 15 || v.Length > 34) throw new ArgumentException("IBAN length invalid.", nameof(value));
            if (!char.IsLetter(v[0]) || !char.IsLetter(v[1])) throw new ArgumentException("IBAN must start with country letters.");
            _value = v;
        }

        public bool Equals(Iban? other) => other is not null && _value == other._value;
        public override bool Equals(object? obj) => Equals(obj as Iban);
        public override int GetHashCode() => _value.GetHashCode(StringComparison.Ordinal);
        public override string ToString() => _value;
    }
}
