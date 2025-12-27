using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities
{
    /// <summary>Kredi kartı başvurusu.</summary>
    public sealed class CreditCardApplication : Entity
    {
        private CreditCardApplication() { }

        public Guid CustomerId { get; private set; }
        public decimal RequestedLimit { get; private set; }
        public decimal? ApprovedLimit { get; private set; }
        public decimal MonthlyIncome { get; private set; }
        public ApplicationStatus Status { get; private set; }
        public string? RejectionReason { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public Guid? ProcessedByAdminId { get; private set; }

        public CreditCardApplication(Guid customerId, decimal requestedLimit, decimal monthlyIncome)
        {
            if (requestedLimit <= 0) throw new ArgumentException("Talep edilen limit 0'dan büyük olmalı.", nameof(requestedLimit));
            if (monthlyIncome <= 0) throw new ArgumentException("Aylık gelir 0'dan büyük olmalı.", nameof(monthlyIncome));
            
            CustomerId = customerId;
            RequestedLimit = requestedLimit;
            MonthlyIncome = monthlyIncome;
            Status = ApplicationStatus.Pending;
        }

        /// <summary>Başvuruyu onayla ve limit belirle.</summary>
        public void Approve(decimal approvedLimit, Guid adminId)
        {
            if (Status != ApplicationStatus.Pending)
                throw new InvalidOperationException("Sadece bekleyen başvurular onaylanabilir.");
            if (approvedLimit <= 0)
                throw new ArgumentException("Onaylanan limit 0'dan büyük olmalı.", nameof(approvedLimit));

            ApprovedLimit = approvedLimit;
            Status = ApplicationStatus.Approved;
            ProcessedAt = DateTime.UtcNow;
            ProcessedByAdminId = adminId;
            TouchUpdated();
        }

        /// <summary>Başvuruyu reddet.</summary>
        public void Reject(string reason, Guid adminId)
        {
            if (Status != ApplicationStatus.Pending)
                throw new InvalidOperationException("Sadece bekleyen başvurular reddedilebilir.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Red nedeni gerekli.", nameof(reason));

            RejectionReason = reason.Trim();
            Status = ApplicationStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
            ProcessedByAdminId = adminId;
            TouchUpdated();
        }

        /// <summary>Başvuruyu iptal et (müşteri tarafından).</summary>
        public void Cancel()
        {
            if (Status != ApplicationStatus.Pending)
                throw new InvalidOperationException("Sadece bekleyen başvurular iptal edilebilir.");

            Status = ApplicationStatus.Cancelled;
            TouchUpdated();
        }
    }
}
