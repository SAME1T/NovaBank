using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("bank_audit_logs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ActorCustomerId);

        builder.Property(e => e.ActorRole)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Action)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasMaxLength(32);

        builder.Property(e => e.EntityId)
            .HasMaxLength(64);

        builder.Property(e => e.Summary)
            .HasMaxLength(256);

        builder.Property(e => e.MetadataJson)
            .HasColumnType("text");

        builder.Property(e => e.IpAddress)
            .HasMaxLength(64);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(256);

        builder.Property(e => e.Success)
            .IsRequired();

        builder.Property(e => e.ErrorCode)
            .HasMaxLength(64);

        // Indexes
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_bank_audit_logs_CreatedAt");

        builder.HasIndex(e => e.ActorCustomerId)
            .HasDatabaseName("IX_bank_audit_logs_ActorCustomerId");

        builder.HasIndex(e => e.Action)
            .HasDatabaseName("IX_bank_audit_logs_Action");
    }
}

