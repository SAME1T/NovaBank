using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class BillInstitutionConfig : IEntityTypeConfiguration<BillInstitution>
{
    public void Configure(EntityTypeBuilder<BillInstitution> builder)
    {
        builder.ToTable("bill_institutions");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.Code)
            .HasColumnName("code")
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
        
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(e => e.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);
        
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(e => e.Category);
    }
}
