using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class CardConfig : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> b)
    {
        b.ToTable("bank_cards");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        b.Property(x => x.AccountId).IsRequired();
        b.Property(x => x.MaskedPan).HasMaxLength(19);
        b.Property(x => x.ExpiryMonth);
        b.Property(x => x.ExpiryYear);
        b.Property(x => x.CardType).HasConversion<int>();
        b.Property(x => x.CardStatus).HasConversion<int>();
        b.Property(x => x.CreditLimit).HasColumnType("decimal(18,2)");
        b.Property(x => x.AvailableLimit).HasColumnType("decimal(18,2)");

        b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
    }
}
