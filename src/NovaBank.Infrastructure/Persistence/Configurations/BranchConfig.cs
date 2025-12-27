using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class BranchConfig : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
        
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(e => e.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.District)
            .HasColumnName("district")
            .HasMaxLength(100);
        
        builder.Property(e => e.Address)
            .HasColumnName("address");
        
        builder.Property(e => e.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);
        
        builder.Property(e => e.ManagerId)
            .HasColumnName("manager_id");
        
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
