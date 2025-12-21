using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfig : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("bank_password_reset_tokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CustomerId)
            .IsRequired();

        builder.Property(e => e.TargetEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.AttemptCount)
            .IsRequired();

        builder.Property(e => e.IsUsed)
            .IsRequired();

        builder.Property(e => e.UsedAt);

        builder.Property(e => e.RequestedIp)
            .HasMaxLength(64);

        builder.Property(e => e.RequestedUserAgent)
            .HasMaxLength(256);

        // Foreign Key
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("IX_bank_password_reset_tokens_CustomerId");

        builder.HasIndex(e => e.ExpiresAt)
            .HasDatabaseName("IX_bank_password_reset_tokens_ExpiresAt");

        builder.HasIndex(e => e.IsUsed)
            .HasDatabaseName("IX_bank_password_reset_tokens_IsUsed");

        // Composite index for performance (CustomerId + IsUsed + ExpiresAt)
        builder.HasIndex(e => new { e.CustomerId, e.IsUsed, e.ExpiresAt })
            .HasDatabaseName("IX_bank_password_reset_tokens_CustomerId_IsUsed_ExpiresAt");
    }
}

