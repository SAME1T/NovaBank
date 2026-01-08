using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
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
        public bool IsApproved { get; private set; } = false;
        public UserRole Role { get; private set; } = UserRole.Customer;
        
        // Şube ilişkisi
        public Guid? BranchId { get; private set; }
        
        // KYC alanları
        public RiskLevel RiskLevel { get; private set; } = RiskLevel.Low;
        public bool KycCompleted { get; private set; } = false;
        public DateTime? KycCompletedAt { get; private set; }
        
        // Güvenlik alanları
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginCount { get; private set; } = 0;
        public DateTime? LockedUntil { get; private set; }

        public Customer(NationalId nationalId, string firstName, string lastName, string email, string phone, string password, UserRole role = UserRole.Customer)
        {
            NationalId = nationalId ?? throw new ArgumentNullException(nameof(nationalId));
            FirstName = ValidateName(firstName, nameof(firstName));
            LastName = ValidateName(lastName, nameof(lastName));
            Email = ValidateEmail(email);
            Phone = phone ?? string.Empty;
            PasswordHash = HashPassword(password);
            Role = role;
        }

        /// <summary>Admin tarafından hesap onayı.</summary>
        public void Approve()
        {
            IsApproved = true;
            TouchUpdated();
        }

        /// <summary>Admin tarafından hesap reddi.</summary>
        public void Reject()
        {
            IsApproved = false;
            IsActive = false;
            TouchUpdated();
        }

        /// <summary>Deactivate the customer.</summary>
        public void Deactivate()
        {
            IsActive = false;
            TouchUpdated();
        }

        /// <summary>Activate the customer.</summary>
        public void Activate()
        {
            IsActive = true;
            TouchUpdated();
        }

        /// <summary>Update contact information.</summary>
        public void UpdateContact(string email, string phone)
        {
            Email = ValidateEmail(email);
            Phone = phone ?? string.Empty;
            TouchUpdated();
        }

        /// <summary>Update password. The password will be hashed before storing.</summary>
        public void UpdatePassword(string newPassword)
        {
            PasswordHash = HashPassword(newPassword);
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

        public static string HashPassword(string password)
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

        /// <summary>Şube ata.</summary>
        public void AssignBranch(Guid branchId)
        {
            BranchId = branchId;
            TouchUpdated();
        }

        /// <summary>KYC tamamlandı olarak işaretle.</summary>
        public void CompleteKyc(RiskLevel riskLevel = RiskLevel.Low)
        {
            KycCompleted = true;
            KycCompletedAt = DateTime.UtcNow;
            RiskLevel = riskLevel;
            TouchUpdated();
        }

        /// <summary>Risk seviyesini güncelle.</summary>
        public void UpdateRiskLevel(RiskLevel riskLevel)
        {
            RiskLevel = riskLevel;
            TouchUpdated();
        }

        /// <summary>Başarılı giriş kaydı.</summary>
        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginCount = 0;
            LockedUntil = null;
            TouchUpdated();
        }

        /// <summary>Başarısız giriş kaydı.</summary>
        public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 30)
        {
            FailedLoginCount++;
            if (FailedLoginCount >= maxAttempts)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            }
            TouchUpdated();
        }

        /// <summary>Hesap kilitli mi kontrol et.</summary>
        public bool IsLocked => LockedUntil.HasValue && DateTime.UtcNow < LockedUntil.Value;

        /// <summary>Kilidi kaldır.</summary>
        public void Unlock()
        {
            FailedLoginCount = 0;
            LockedUntil = null;
            TouchUpdated();
        }

        /// <summary>Tam ad.</summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>Kullanıcı rolünü güncelle (sadece Admin tarafından).</summary>
        public void UpdateRole(UserRole newRole)
        {
            Role = newRole;
            TouchUpdated();
        }
    }
}
