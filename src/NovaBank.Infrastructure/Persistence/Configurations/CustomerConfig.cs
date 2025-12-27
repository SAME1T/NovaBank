using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Infrastructure.Persistence.Converters;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("bank_customers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt);
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken().IsRequired(false);

        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(20);
        b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.IsApproved).HasDefaultValue(false);
        b.Property(x => x.Role)
            .HasConversion<int>()
            .HasDefaultValue(UserRole.Customer)
            .IsRequired();

        b.Property(x => x.NationalId)
            .HasConversion(new NationalIdConverter())
            .HasColumnName("national_id")
            .HasMaxLength(11)
            .IsRequired();

        // Şube ilişkisi
        b.Property(x => x.BranchId).HasColumnName("branch_id");

        // KYC alanları
        b.Property(x => x.RiskLevel)
            .HasColumnName("risk_level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(RiskLevel.Low);
        b.Property(x => x.KycCompleted).HasColumnName("kyc_completed").HasDefaultValue(false);
        b.Property(x => x.KycCompletedAt).HasColumnName("kyc_completed_at");

        // Güvenlik alanları
        b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        b.Property(x => x.FailedLoginCount).HasColumnName("failed_login_count").HasDefaultValue(0);
        b.Property(x => x.LockedUntil).HasColumnName("locked_until");

        b.HasIndex(x => x.NationalId).IsUnique();
        b.HasIndex(x => x.Email);
        b.HasIndex(x => x.BranchId);
    }
}
