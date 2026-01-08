using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class CurrencyPositionConfig : IEntityTypeConfiguration<CurrencyPosition>
{
    public void Configure(EntityTypeBuilder<CurrencyPosition> builder)
    {
        builder.ToTable("currency_positions");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName("id");
            
        builder.Property(p => p.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
            
        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasConversion<string>()
            .IsRequired();
            
        builder.Property(p => p.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 6)
            .IsRequired();
            
        builder.Property(p => p.AverageCostRate)
            .HasColumnName("average_cost_rate")
            .HasPrecision(18, 6)
            .IsRequired();
            
        builder.Property(p => p.TotalCostTry)
            .HasColumnName("total_cost_try")
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");
            
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");
            
        // Unique constraint: bir müşteri her döviz için tek pozisyon
        builder.HasIndex(p => new { p.CustomerId, p.Currency })
            .IsUnique()
            .HasDatabaseName("ix_currency_positions_customer_currency");
            
        builder.HasIndex(p => p.CustomerId)
            .HasDatabaseName("ix_currency_positions_customer");
    }
}
