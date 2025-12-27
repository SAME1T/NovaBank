using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class LoanConfig : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> b)
    {
        b.ToTable("bank_loans");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.CustomerId).IsRequired();
        b.OwnsOne(x => x.Principal, mb =>
        {
            mb.Property(p => p.Amount).HasColumnName("principal_amount").HasColumnType("decimal(18,2)");
            mb.Property(p => p.Currency).HasColumnName("principal_currency").HasConversion<int>();
        });
        b.Property(x => x.InterestRateAnnual).HasColumnType("decimal(5,4)"); // ör: 0.2495
        b.Property(x => x.TermMonths);
        b.Property(x => x.StartDate);
        b.Property(x => x.Status).HasConversion<int>();

        // Onay alanları
        b.Property(x => x.IsApproved).HasColumnName("is_approved").HasDefaultValue(false);
        b.Property(x => x.ApprovedById).HasColumnName("approved_by_id");
        b.Property(x => x.ApprovedAt).HasColumnName("approved_at");
        b.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);

        // Ödeme alanları
        b.Property(x => x.DisbursementAccountId).HasColumnName("disbursement_account_id");
        b.Property(x => x.RemainingPrincipal).HasColumnName("remaining_principal").HasColumnType("decimal(18,2)");
        b.Property(x => x.NextPaymentDate).HasColumnName("next_payment_date");
        b.Property(x => x.NextPaymentAmount).HasColumnName("next_payment_amount").HasColumnType("decimal(18,2)");
        b.Property(x => x.PaidInstallments).HasColumnName("paid_installments").HasDefaultValue(0);

        b.HasIndex(x => new { x.CustomerId, x.Status });
    }
}

