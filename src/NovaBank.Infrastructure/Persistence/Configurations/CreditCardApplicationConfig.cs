using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class CreditCardApplicationConfig : IEntityTypeConfiguration<CreditCardApplication>
{
    public void Configure(EntityTypeBuilder<CreditCardApplication> b)
    {
        b.ToTable("bank_credit_card_applications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.CustomerId).IsRequired();
        b.Property(x => x.RequestedLimit).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.ApprovedLimit).HasColumnType("decimal(18,2)");
        b.Property(x => x.MonthlyIncome).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.Status)
            .HasConversion<int>()
            .HasDefaultValue(ApplicationStatus.Pending)
            .IsRequired();
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.ProcessedAt);
        b.Property(x => x.ProcessedByAdminId);

        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.ProcessedByAdminId).OnDelete(DeleteBehavior.SetNull);
    }
}
