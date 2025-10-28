using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfig : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.ToTable("bank_transactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.AccountId).IsRequired();
        b.HasIndex(x => new { x.AccountId, x.CreatedAt });

        b.OwnsOne(x => x.Amount, mb =>
        {
            mb.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            mb.Property(p => p.Currency).HasColumnName("currency").HasConversion<int>();
        });

        b.Property(x => x.Direction).HasConversion<int>();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.ReferenceCode).HasMaxLength(64);
        b.Property(x => x.TransactionDate).IsRequired();

        b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
    }
}
