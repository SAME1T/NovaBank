using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class KycVerificationConfig : IEntityTypeConfiguration<KycVerification>
{
    public void Configure(EntityTypeBuilder<KycVerification> builder)
    {
        builder.ToTable("kyc_verifications");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        
        builder.Property(e => e.VerificationType)
            .HasColumnName("verification_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.DocumentPath)
            .HasColumnName("document_path")
            .HasMaxLength(500);
        
        builder.Property(e => e.VerifiedById)
            .HasColumnName("verified_by_id");
        
        builder.Property(e => e.VerifiedAt)
            .HasColumnName("verified_at");
        
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
        
        builder.HasIndex(e => new { e.CustomerId, e.VerificationType }).IsUnique();
        builder.HasIndex(e => e.Status);
    }
}
