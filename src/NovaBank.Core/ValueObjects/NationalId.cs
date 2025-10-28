using System;
using System.Text.RegularExpressions;

namespace NovaBank.Core.ValueObjects
{
    /// <summary>Turkish national identity number (11 digits) with basic validation.</summary>
    public sealed class NationalId : IEquatable<NationalId>
    {
        private readonly string _value;
        public string Value => _value;

        public NationalId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", nameof(value));
            var v = value.Trim();
            if (v.Length != 11 || !Regex.IsMatch(v, "^\\d{11}$"))
                throw new ArgumentException("NationalId must be 11 digits.", nameof(value));
            _value = v;
        }

        public bool Equals(NationalId? other) => other is not null && _value == other._value;
        public override bool Equals(object? obj) => Equals(obj as NationalId);
        public override int GetHashCode() => _value.GetHashCode(StringComparison.Ordinal);
        public override string ToString() => _value;
    }
}
