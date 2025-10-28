using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Infrastructure.Persistence.Converters;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("bank_accounts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.CustomerId).IsRequired();
        b.HasIndex(x => new { x.CustomerId, x.Id });

        b.Property(x => x.AccountNo)
            .HasConversion(new AccountNoConverter())
            .HasColumnName("account_no")
            .IsRequired();

        b.Property(x => x.Iban)
            .HasConversion(new IbanConverter())
            .HasColumnName("iban")
            .HasMaxLength(34)
            .IsRequired();

        b.Property(x => x.Currency)
            .HasConversion<int>()
            .HasDefaultValue(Currency.TRY)
            .IsRequired();

        // Money as owned (Balance)
        b.OwnsOne(x => x.Balance, mb =>
        {
            mb.Property(p => p.Amount).HasColumnName("balance_amount").HasColumnType("decimal(18,2)");
            mb.Property(p => p.Currency).HasColumnName("balance_currency").HasConversion<int>();
        });

        b.Property(x => x.OverdraftLimit).HasColumnType("decimal(18,2)").HasDefaultValue(0);

        b.HasIndex(x => x.AccountNo).IsUnique();
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
