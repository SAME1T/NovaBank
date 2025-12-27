using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class CommissionConfig : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("commissions");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.CommissionType)
            .HasColumnName("commission_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(e => e.Description)
            .HasColumnName("description");
        
        builder.Property(e => e.Currency)
            .HasColumnName("currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
        
        builder.Property(e => e.FixedAmount)
            .HasColumnName("fixed_amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.PercentageRate)
            .HasColumnName("percentage_rate")
            .HasPrecision(8, 5)
            .HasDefaultValue(0);
        
        builder.Property(e => e.MinAmount)
            .HasColumnName("min_amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.MaxAmount)
            .HasColumnName("max_amount")
            .HasPrecision(18, 2);
        
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        builder.Property(e => e.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();
        
        builder.Property(e => e.ValidUntil)
            .HasColumnName("valid_until");
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(e => new { e.CommissionType, e.IsActive });
    }
}
