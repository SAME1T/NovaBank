using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class PaymentOrderConfig : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> b)
    {
        b.ToTable("bank_payment_orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.AccountId).IsRequired();
        b.Property(x => x.PayeeName).HasMaxLength(200);
        b.Property(x => x.PayeeIban)
            .HasConversion(new NovaBank.Infrastructure.Persistence.Converters.IbanConverter())
            .HasColumnName("payee_iban")
            .HasMaxLength(34);
        b.Property(x => x.CronExpr).HasMaxLength(64);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.NextRunAt);

        b.OwnsOne(x => x.Amount, mb =>
        {
            mb.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            mb.Property(p => p.Currency).HasColumnName("currency").HasConversion<int>();
        });
    }
}
