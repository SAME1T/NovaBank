using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class TransactionLimitConfig : IEntityTypeConfiguration<TransactionLimit>
{
    public void Configure(EntityTypeBuilder<TransactionLimit> builder)
    {
        builder.ToTable("transaction_limits");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.LimitType)
            .HasColumnName("limit_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        
        builder.Property(e => e.Scope)
            .HasColumnName("scope")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.ScopeId)
            .HasColumnName("scope_id");
        
        builder.Property(e => e.ScopeRole)
            .HasColumnName("scope_role")
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(e => e.Currency)
            .HasColumnName("currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
        
        builder.Property(e => e.MinAmount)
            .HasColumnName("min_amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.MaxAmount)
            .HasColumnName("max_amount")
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.RequiresApprovalAbove)
            .HasColumnName("requires_approval_above")
            .HasPrecision(18, 2);
        
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(e => new { e.LimitType, e.Scope, e.ScopeId, e.Currency });
    }
}
