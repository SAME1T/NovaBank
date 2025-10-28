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

        b.HasIndex(x => new { x.CustomerId, x.Status });
    }
}
