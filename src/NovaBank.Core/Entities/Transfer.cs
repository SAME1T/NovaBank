using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents a money transfer between accounts.</summary>
    public sealed class Transfer : Entity
    {
        private Transfer() { }
        public Guid FromAccountId { get; private set; }
        public Guid? ToAccountId { get; private set; }
        public Money Amount { get; private set; }
        public TransferChannel Channel { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? ExternalIban { get; private set; }
        public Guid? ReversalOfTransferId { get; private set; }
        public Guid? ReversedByTransferId { get; private set; }
        public DateTime? ReversedAt { get; private set; }

        public Transfer(Guid fromAccountId, Guid? toAccountId, Money amount, TransferChannel channel, string? externalIban = null, Guid? reversalOfTransferId = null)
        {
            if (amount is null) throw new ArgumentNullException(nameof(amount));
            if (amount.Amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            Amount = amount;
            Channel = channel;
            ExternalIban = externalIban;
            ReversalOfTransferId = reversalOfTransferId;
            Status = PaymentStatus.Executed;
        }

        /// <summary>Marks the transfer as executed.</summary>
        public void MarkExecuted()
        {
            Status = PaymentStatus.Executed;
            TouchUpdated();
        }

        /// <summary>Marks the transfer as failed with a reason.</summary>
        public void MarkFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            TouchUpdated();
        }

        /// <summary>Marks the transfer as reversed by another transfer.</summary>
        public void MarkReversed(Guid reversalTransferId, DateTime now)
        {
            if (ReversedByTransferId != null)
                throw new Exceptions.DomainException("Transfer already reversed");
            ReversedByTransferId = reversalTransferId;
            ReversedAt = now;
            TouchUpdated();
        }
    }
}
