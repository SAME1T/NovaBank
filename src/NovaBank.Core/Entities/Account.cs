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

        public Account(Guid customerId, AccountNo accountNo, Iban iban, Currency currency, Money openingBalance, decimal overdraftLimit)
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
            if (!CanWithdraw(amount)) throw new InvalidOperationException("Insufficient funds.");
            Balance = Balance.Subtract(amount);
            TouchUpdated();
        }
    }
}
