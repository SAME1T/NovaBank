using Microsoft.EntityFrameworkCore;
using NovaBank.Core.Entities;
using NovaBank.Infrastructure.Persistence.Configurations;

namespace NovaBank.Infrastructure.Persistence;

public sealed class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    // Mevcut entity'ler
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<CreditCardApplication> CreditCardApplications => Set<CreditCardApplication>();

    // Yeni entity'ler
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<TransactionLimit> TransactionLimits => Set<TransactionLimit>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<KycVerification> KycVerifications => Set<KycVerification>();
    public DbSet<BillInstitution> BillInstitutions => Set<BillInstitution>();
    public DbSet<BillPayment> BillPayments => Set<BillPayment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<CurrencyPosition> CurrencyPositions => Set<CurrencyPosition>();
    public DbSet<CurrencyTransaction> CurrencyTransactions => Set<CurrencyTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mevcut konfigürasyonlar
        modelBuilder.ApplyConfiguration(new CustomerConfig());
        modelBuilder.ApplyConfiguration(new AccountConfig());
        modelBuilder.ApplyConfiguration(new TransactionConfig());
        modelBuilder.ApplyConfiguration(new CardConfig());
        modelBuilder.ApplyConfiguration(new TransferConfig());
        modelBuilder.ApplyConfiguration(new PaymentOrderConfig());
        modelBuilder.ApplyConfiguration(new LoanConfig());
        modelBuilder.ApplyConfiguration(new AuditLogConfig());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfig());
        modelBuilder.ApplyConfiguration(new CreditCardApplicationConfig());

        // Yeni konfigürasyonlar
        modelBuilder.ApplyConfiguration(new BranchConfig());
        modelBuilder.ApplyConfiguration(new TransactionLimitConfig());
        modelBuilder.ApplyConfiguration(new CommissionConfig());
        modelBuilder.ApplyConfiguration(new ApprovalWorkflowConfig());
        modelBuilder.ApplyConfiguration(new KycVerificationConfig());
        modelBuilder.ApplyConfiguration(new BillInstitutionConfig());
        modelBuilder.ApplyConfiguration(new BillPaymentConfig());
        modelBuilder.ApplyConfiguration(new NotificationConfig());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfig());
        modelBuilder.ApplyConfiguration(new ExchangeRateConfig());
        modelBuilder.ApplyConfiguration(new CurrencyPositionConfig());
        modelBuilder.ApplyConfiguration(new CurrencyTransactionConfig());

        base.OnModelCreating(modelBuilder);
    }
}

