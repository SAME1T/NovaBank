using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class ApprovalWorkflowConfig : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("approval_workflows");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        
        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();
        
        builder.Property(e => e.RequestedById)
            .HasColumnName("requested_by_id")
            .IsRequired();
        
        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);
        
        builder.Property(e => e.Currency)
            .HasColumnName("currency")
            .HasConversion<string>()
            .HasMaxLength(3);
        
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.RequiredRole)
            .HasColumnName("required_role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.ApprovedById)
            .HasColumnName("approved_by_id");
        
        builder.Property(e => e.ApprovedAt)
            .HasColumnName("approved_at");
        
        builder.Property(e => e.RejectionReason)
            .HasColumnName("rejection_reason");
        
        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at");
        
        builder.Property(e => e.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("jsonb");
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(e => new { e.EntityType, e.Status });
        builder.HasIndex(e => e.RequestedById);
        builder.HasIndex(e => e.Status);
    }
}
