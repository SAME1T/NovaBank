using System;
using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Core.Entities
{
    /// <summary>Represents a loan contract.</summary>
    public sealed class Loan : Entity
    {
        private Loan() { }
        public Guid CustomerId { get; private set; }
        public Money Principal { get; private set; }
        public decimal InterestRateAnnual { get; private set; }
        public int TermMonths { get; private set; }
        public DateTime StartDate { get; private set; }
        public LoanStatus Status { get; private set; }
        
        // Onay alanları
        public bool IsApproved { get; private set; } = false;
        public Guid? ApprovedById { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? RejectionReason { get; private set; }
        
        // Ödeme alanları
        public Guid? DisbursementAccountId { get; private set; }
        public decimal RemainingPrincipal { get; private set; }
        public DateTime? NextPaymentDate { get; private set; }
        public decimal? NextPaymentAmount { get; private set; }
        public int PaidInstallments { get; private set; } = 0;

        public Loan(Guid customerId, Money principal, decimal interestRateAnnual, int termMonths, DateTime startDate, Guid? disbursementAccountId = null)
        {
            if (principal is null) throw new ArgumentNullException(nameof(principal));
            if (interestRateAnnual < 0) throw new ArgumentException("Interest rate cannot be negative.", nameof(interestRateAnnual));
            if (termMonths <= 0) throw new ArgumentException("Term must be positive.", nameof(termMonths));
            CustomerId = customerId;
            Principal = principal;
            InterestRateAnnual = interestRateAnnual;
            TermMonths = termMonths;
            StartDate = startDate;
            Status = LoanStatus.Draft;
            DisbursementAccountId = disbursementAccountId;
            RemainingPrincipal = principal.Amount;
        }

        /// <summary>Calculates the monthly installment using the annuity formula.</summary>
        public decimal MonthlyInstallment(decimal? insuranceRate = null)
        {
            decimal monthlyRate = InterestRateAnnual / 12m;
            decimal p = Principal.Amount * (insuranceRate.HasValue ? (1m + insuranceRate.Value) : 1m);
            if (monthlyRate == 0m)
            {
                return Math.Round(p / TermMonths, 2, MidpointRounding.AwayFromZero);
            }
            decimal r = monthlyRate;
            decimal numerator = p * r;
            decimal denominator = 1m - (decimal)Math.Pow((double)(1m + r), -TermMonths);
            var installment = numerator / denominator;
            return Math.Round(installment, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Krediyi onayla.</summary>
        public void Approve(Guid approvedById)
        {
            if (Status != LoanStatus.Draft)
                throw new InvalidOperationException("Sadece taslak durumundaki krediler onaylanabilir.");
            
            IsApproved = true;
            ApprovedById = approvedById;
            ApprovedAt = DateTime.UtcNow;
            Status = LoanStatus.Active;
            NextPaymentDate = StartDate.AddMonths(1);
            NextPaymentAmount = MonthlyInstallment();
            TouchUpdated();
        }

        /// <summary>Krediyi reddet.</summary>
        public void Reject(Guid rejectedById, string reason)
        {
            if (Status != LoanStatus.Draft)
                throw new InvalidOperationException("Sadece taslak durumundaki krediler reddedilebilir.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Red nedeni gerekli.", nameof(reason));
            
            IsApproved = false;
            ApprovedById = rejectedById;
            ApprovedAt = DateTime.UtcNow;
            RejectionReason = reason.Trim();
            Status = LoanStatus.Closed;
            TouchUpdated();
        }

        /// <summary>Taksit ödemesi yap.</summary>
        public void MakePayment(decimal amount)
        {
            if (Status != LoanStatus.Active)
                throw new InvalidOperationException("Sadece aktif kredilere ödeme yapılabilir.");
            if (amount <= 0)
                throw new ArgumentException("Ödeme tutarı pozitif olmalı.", nameof(amount));
            
            RemainingPrincipal = Math.Max(0, RemainingPrincipal - amount);
            PaidInstallments++;
            
            if (RemainingPrincipal <= 0 || PaidInstallments >= TermMonths)
            {
                Status = LoanStatus.Closed;
                NextPaymentDate = null;
                NextPaymentAmount = null;
            }
            else
            {
                NextPaymentDate = NextPaymentDate?.AddMonths(1) ?? DateTime.UtcNow.AddMonths(1);
                NextPaymentAmount = MonthlyInstallment();
            }
            
            TouchUpdated();
        }

        /// <summary>Closes the loan.</summary>
        public void Close()
        {
            Status = LoanStatus.Closed;
            TouchUpdated();
        }

        /// <summary>Marks the loan as defaulted.</summary>
        public void Default()
        {
            Status = LoanStatus.Defaulted;
            TouchUpdated();
        }

        /// <summary>Kalan taksit sayısı.</summary>
        public int RemainingInstallments => Math.Max(0, TermMonths - PaidInstallments);
        
        /// <summary>Toplam geri ödeme tutarı.</summary>
        public decimal TotalRepayment => MonthlyInstallment() * TermMonths;
    }
}
