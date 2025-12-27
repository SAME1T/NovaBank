using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents a bank account.</summary>
    public sealed class Account : Entity
    {
        // EF Core parameterless constructor
        private Account() { }
        public Guid CustomerId { get; private set; }
        public AccountNo AccountNo { get; private set; }
        public Iban Iban { get; private set; }
        public Currency Currency { get; private set; }
        public Money Balance { get; private set; }
        public decimal OverdraftLimit { get; private set; }
        public AccountStatus Status { get; private set; } = AccountStatus.Active;
        
        // Şube ilişkisi
        public Guid? BranchId { get; private set; }
        
        // Onay alanları
        public bool IsApproved { get; private set; } = false;
        public Guid? ApprovedById { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        
        // Hesap türü ve faiz
        public AccountType AccountType { get; private set; } = AccountType.Checking;
        public decimal InterestRate { get; private set; } = 0;

        public Account(Guid customerId, AccountNo accountNo, Iban iban, Currency currency, Money openingBalance, decimal overdraftLimit, AccountStatus status = AccountStatus.Active, AccountType accountType = AccountType.Checking)
        {
            if (overdraftLimit < 0) throw new ArgumentException("OverdraftLimit cannot be negative.", nameof(overdraftLimit));
            if (openingBalance is null) throw new ArgumentNullException(nameof(openingBalance));
            if (openingBalance.Currency != currency) throw new ArgumentException("Opening balance currency mismatch.");
            CustomerId = customerId;
            AccountNo = accountNo;
            Iban = iban;
            Currency = currency;
            Balance = openingBalance;
            OverdraftLimit = overdraftLimit;
            Status = status;
            AccountType = accountType;
        }

        /// <summary>Updates the overdraft limit.</summary>
        public void UpdateOverdraftLimit(decimal newLimit)
        {
            if (newLimit < 0) throw new ArgumentException("OverdraftLimit cannot be negative.", nameof(newLimit));
            OverdraftLimit = newLimit;
            TouchUpdated();
        }

        /// <summary>Freezes the account.</summary>
        public void Freeze()
        {
            if (Status == AccountStatus.Closed) throw new InvalidOperationException("Closed account cannot be frozen.");
            Status = AccountStatus.Frozen;
            TouchUpdated();
        }

        /// <summary>Activates the account.</summary>
        public void Activate()
        {
            if (Status == AccountStatus.Closed) throw new InvalidOperationException("Closed account cannot be activated.");
            Status = AccountStatus.Active;
            TouchUpdated();
        }

        /// <summary>Closes the account.</summary>
        public void Close()
        {
            Status = AccountStatus.Closed;
            TouchUpdated();
        }

        /// <summary>Deposits the specified amount. Requires same currency.</summary>
        public void Deposit(Money amount)
        {
            if (amount is null) throw new ArgumentNullException(nameof(amount));
            if (amount.Currency != Currency) throw new ArgumentException("Currency mismatch.");
            Balance = Balance.Add(amount);
            TouchUpdated();
        }

        /// <summary>Returns true if the account can withdraw the specified amount considering overdraft.</summary>
        public bool CanWithdraw(Money amount)
        {
            if (amount is null) return false;
            if (amount.Currency != Currency) return false;
            return (Balance.Amount + OverdraftLimit) >= amount.Amount;
        }

        /// <summary>Withdraws the specified amount or throws if not allowed.</summary>
        public void Withdraw(Money amount)
        {
            // Sistem kasa hesabı için bakiye kontrolü yapma (sonsuz para kaynağı)
            bool isSystemCashAccount = Iban.Value.StartsWith("TR00CASH");
            
            if (!isSystemCashAccount && !CanWithdraw(amount)) 
                throw new InvalidOperationException("Insufficient funds.");
            
            Balance = Balance.Subtract(amount);
            TouchUpdated();
        }

        /// <summary>Hesabı onayla.</summary>
        public void Approve(Guid approvedById)
        {
            IsApproved = true;
            ApprovedById = approvedById;
            ApprovedAt = DateTime.UtcNow;
            TouchUpdated();
        }

        /// <summary>Şube ata.</summary>
        public void AssignBranch(Guid branchId)
        {
            BranchId = branchId;
            TouchUpdated();
        }

        /// <summary>Faiz oranını güncelle.</summary>
        public void UpdateInterestRate(decimal rate)
        {
            if (rate < 0) throw new ArgumentException("Faiz oranı negatif olamaz.", nameof(rate));
            InterestRate = rate;
            TouchUpdated();
        }
    }
}
