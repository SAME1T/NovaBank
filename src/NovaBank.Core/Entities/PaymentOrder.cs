using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents a standing payment order.</summary>
    public sealed class PaymentOrder : Entity
    {
        private PaymentOrder() { }
        public Guid AccountId { get; private set; }
        public string PayeeName { get; private set; }
        public Iban PayeeIban { get; private set; }
        public Money Amount { get; private set; }
        public string CronExpr { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime NextRunAt { get; private set; }

        public PaymentOrder(Guid accountId, string payeeName, Iban payeeIban, Money amount, string cronExpr, DateTime nextRunAt)
        {
            if (string.IsNullOrWhiteSpace(payeeName)) throw new ArgumentException("PayeeName is required.", nameof(payeeName));
            if (string.IsNullOrWhiteSpace(cronExpr)) throw new ArgumentException("CronExpr is required.", nameof(cronExpr));
            AccountId = accountId;
            PayeeName = payeeName.Trim();
            PayeeIban = payeeIban ?? throw new ArgumentNullException(nameof(payeeIban));
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            CronExpr = cronExpr.Trim();
            Status = PaymentStatus.Scheduled;
            NextRunAt = nextRunAt;
        }

        /// <summary>Schedules next run time.</summary>
        public void ScheduleNext(DateTime executedAt)
        {
            NextRunAt = executedAt;
            TouchUpdated();
        }

        /// <summary>Cancels the standing order.</summary>
        public void Cancel()
        {
            Status = PaymentStatus.Canceled;
            TouchUpdated();
        }
    }
}
