using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfig : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        
        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        builder.HasIndex(e => e.CustomerId).IsUnique();
        
        builder.Property(e => e.TransactionSms)
            .HasColumnName("transaction_sms")
            .HasDefaultValue(true);
        
        builder.Property(e => e.TransactionEmail)
            .HasColumnName("transaction_email")
            .HasDefaultValue(true);
        
        builder.Property(e => e.LoginSms)
            .HasColumnName("login_sms")
            .HasDefaultValue(true);
        
        builder.Property(e => e.LoginEmail)
            .HasColumnName("login_email")
            .HasDefaultValue(true);
        
        builder.Property(e => e.MarketingSms)
            .HasColumnName("marketing_sms")
            .HasDefaultValue(false);
        
        builder.Property(e => e.MarketingEmail)
            .HasColumnName("marketing_email")
            .HasDefaultValue(false);
        
        builder.Property(e => e.SecurityAlertSms)
            .HasColumnName("security_alert_sms")
            .HasDefaultValue(true);
        
        builder.Property(e => e.SecurityAlertEmail)
            .HasColumnName("security_alert_email")
            .HasDefaultValue(true);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
