using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class ExchangeRateConfig : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.BaseCurrency)
            .HasColumnName("base_currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
        
        builder.Property(e => e.TargetCurrency)
            .HasColumnName("target_currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
        
        builder.Property(e => e.BuyRate)
            .HasColumnName("buy_rate")
            .HasPrecision(18, 6)
            .IsRequired();
        
        builder.Property(e => e.SellRate)
            .HasColumnName("sell_rate")
            .HasPrecision(18, 6)
            .IsRequired();
        
        builder.Property(e => e.EffectiveDate)
            .HasColumnName("effective_date")
            .IsRequired();
        
        builder.Property(e => e.Source)
            .HasColumnName("source")
            .HasMaxLength(50)
            .HasDefaultValue("TCMB");
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(e => new { e.BaseCurrency, e.TargetCurrency, e.EffectiveDate }).IsUnique();
    }
}
