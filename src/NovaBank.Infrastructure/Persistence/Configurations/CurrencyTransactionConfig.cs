using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaBank.Core.Entities;

namespace NovaBank.Infrastructure.Persistence.Configurations;

public class CurrencyTransactionConfig : IEntityTypeConfiguration<CurrencyTransaction>
{
    public void Configure(EntityTypeBuilder<CurrencyTransaction> builder)
    {
        builder.ToTable("currency_transactions");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnName("id");
            
        builder.Property(t => t.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
            
        builder.Property(t => t.TransactionType)
            .HasColumnName("transaction_type")
            .HasMaxLength(4)
            .IsRequired();
            
        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasConversion<string>()
            .IsRequired();
            
        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 6)
            .IsRequired();
            
        builder.Property(t => t.ExchangeRate)
            .HasColumnName("exchange_rate")
            .HasPrecision(18, 6)
            .IsRequired();
            
        builder.Property(t => t.RateType)
            .HasColumnName("rate_type")
            .HasMaxLength(4)
            .IsRequired();
            
        builder.Property(t => t.RateSource)
            .HasColumnName("rate_source")
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(t => t.RateDate)
            .HasColumnName("rate_date")
            .IsRequired();
            
        builder.Property(t => t.TryAmount)
            .HasColumnName("try_amount")
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.CommissionTry)
            .HasColumnName("commission_try")
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.NetTryAmount)
            .HasColumnName("net_try_amount")
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.FromAccountId)
            .HasColumnName("from_account_id")
            .IsRequired();
            
        builder.Property(t => t.ToAccountId)
            .HasColumnName("to_account_id")
            .IsRequired();
            
        builder.Property(t => t.PositionBeforeAmount)
            .HasColumnName("position_before_amount")
            .HasPrecision(18, 6);
            
        builder.Property(t => t.PositionAfterAmount)
            .HasColumnName("position_after_amount")
            .HasPrecision(18, 6);
            
        builder.Property(t => t.AvgCostBefore)
            .HasColumnName("avg_cost_before")
            .HasPrecision(18, 6);
            
        builder.Property(t => t.AvgCostAfter)
            .HasColumnName("avg_cost_after")
            .HasPrecision(18, 6);
            
        builder.Property(t => t.RealizedPnlTry)
            .HasColumnName("realized_pnl_try")
            .HasPrecision(18, 2);
            
        builder.Property(t => t.RealizedPnlPercent)
            .HasColumnName("realized_pnl_percent")
            .HasPrecision(8, 4);
            
        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
            
        builder.Property(t => t.ReferenceCode)
            .HasColumnName("reference_code")
            .HasMaxLength(30)
            .IsRequired();
            
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");
            
        // İndeksler
        builder.HasIndex(t => t.CustomerId)
            .HasDatabaseName("ix_currency_transactions_customer");
            
        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_currency_transactions_date");
            
        builder.HasIndex(t => t.TransactionType)
            .HasDatabaseName("ix_currency_transactions_type");
            
        builder.HasIndex(t => t.ReferenceCode)
            .IsUnique()
            .HasDatabaseName("ix_currency_transactions_reference");
    }
}
