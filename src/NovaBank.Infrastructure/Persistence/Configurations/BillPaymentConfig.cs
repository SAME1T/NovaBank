using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class BillPaymentConfig : IEntityTypeConfiguration<BillPayment>
{
    public void Configure(EntityTypeBuilder<BillPayment> builder)
    {
        builder.ToTable("bill_payments");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.AccountId)
            .HasColumnName("account_id");

        builder.Property(e => e.CardId)
            .HasColumnName("card_id");
        
        builder.Property(e => e.InstitutionId)
            .HasColumnName("institution_id")
            .IsRequired();
        
        builder.Property(e => e.SubscriberNo)
            .HasColumnName("subscriber_no")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.Commission)
            .HasColumnName("commission")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(e => e.ReferenceCode)
            .HasColumnName("reference_code")
            .HasMaxLength(50);
        
        builder.Property(e => e.DueDate)
            .HasColumnName("due_date");
        
        builder.Property(e => e.PaidAt)
            .HasColumnName("paid_at")
            .IsRequired();
        
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.HasIndex(e => e.AccountId);
        builder.HasIndex(e => e.CardId);
        builder.HasIndex(e => e.InstitutionId);
        builder.HasIndex(e => e.ReferenceCode);
    }
}
