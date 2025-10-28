using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NovaBank.Core.Abstractions;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents a bank customer.</summary>
    public sealed class Customer : Entity
    {
        private Customer() { }
        public NationalId NationalId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsActive { get; private set; } = true;

        public Customer(NationalId nationalId, string firstName, string lastName, string email, string phone, string password)
        {
            NationalId = nationalId ?? throw new ArgumentNullException(nameof(nationalId));
            FirstName = ValidateName(firstName, nameof(firstName));
            LastName = ValidateName(lastName, nameof(lastName));
            Email = ValidateEmail(email);
            Phone = phone ?? string.Empty;
            PasswordHash = HashPassword(password);
        }

        /// <summary>Deactivate the customer.</summary>
        public void Deactivate()
        {
            IsActive = false;
            TouchUpdated();
        }

        /// <summary>Update contact information.</summary>
        public void UpdateContact(string email, string phone)
        {
            Email = ValidateEmail(email);
            Phone = phone ?? string.Empty;
            TouchUpdated();
        }

        private static string ValidateName(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", paramName);
            return value.Trim();
        }

        private static string ValidateEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Email is required.", nameof(value));
            var v = value.Trim();
            if (!Regex.IsMatch(v, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email format is invalid.", nameof(value));
            return v;
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required.", nameof(password));
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return PasswordHash == HashPassword(password);
        }
    }
}
