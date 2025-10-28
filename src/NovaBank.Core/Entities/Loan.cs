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

        public Loan(Guid customerId, Money principal, decimal interestRateAnnual, int termMonths, DateTime startDate)
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
    }
}
