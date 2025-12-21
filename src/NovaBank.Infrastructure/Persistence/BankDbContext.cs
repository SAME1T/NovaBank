using Microsoft.EntityFrameworkCore;
using NovaBank.Core.Entities;
using NovaBank.Infrastructure.Persistence.Configurations;

namespace NovaBank.Infrastructure.Persistence;

public sealed class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfig());
        modelBuilder.ApplyConfiguration(new AccountConfig());
        modelBuilder.ApplyConfiguration(new TransactionConfig());
        modelBuilder.ApplyConfiguration(new CardConfig());
        modelBuilder.ApplyConfiguration(new TransferConfig());
        modelBuilder.ApplyConfiguration(new PaymentOrderConfig());
        modelBuilder.ApplyConfiguration(new LoanConfig());
        modelBuilder.ApplyConfiguration(new AuditLogConfig());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfig());
        base.OnModelCreating(modelBuilder);
    }
}
