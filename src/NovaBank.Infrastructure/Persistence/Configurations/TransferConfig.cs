using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class TransferConfig : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> b)
    {
        b.ToTable("bank_transfers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.FromAccountId).IsRequired();
        b.Property(x => x.ToAccountId).IsRequired(false);
        b.Property(x => x.Channel).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();

        b.OwnsOne(x => x.Amount, mb =>
        {
            mb.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            mb.Property(p => p.Currency).HasColumnName("currency").HasConversion<int>();
        });

        b.Property(x => x.ExternalIban).HasMaxLength(34);
        
        b.Property(x => x.ReversalOfTransferId);
        b.Property(x => x.ReversedByTransferId);
        b.Property(x => x.ReversedAt);

        // Indexes
        b.HasIndex(x => x.ReversedByTransferId)
            .IsUnique()
            .HasDatabaseName("IX_bank_transfers_ReversedByTransferId");
        
        b.HasIndex(x => x.ReversalOfTransferId)
            .HasDatabaseName("IX_bank_transfers_ReversalOfTransferId");
    }
}
