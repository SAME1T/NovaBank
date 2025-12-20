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
        b.Property(x => x.Role)
            .HasConversion<int>()
            .HasDefaultValue(UserRole.Customer)
            .IsRequired();

        b.Property(x => x.NationalId)
            .HasConversion(new NationalIdConverter())
            .HasColumnName("national_id")
            .HasMaxLength(11)
            .IsRequired();

        b.HasIndex(x => x.NationalId).IsUnique();
    }
}
