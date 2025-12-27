using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaBank.Application.Common.Email;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Services;
using NovaBank.Infrastructure.Email;
using NovaBank.Infrastructure.Persistence;
using NovaBank.Infrastructure.Persistence.Repositories;
using NovaBank.Infrastructure.Persistence.UnitOfWork;
using NovaBank.Infrastructure.Services;

namespace NovaBank.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString, IConfiguration? configuration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        services.AddDbContext<BankDbContext>(opt =>
            opt.UseNpgsql(connectionString, x => x.MigrationsAssembly(typeof(BankDbContext).Assembly.FullName)));
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        
        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<ICreditCardApplicationRepository, CreditCardApplicationRepository>();
        services.AddScoped<IApprovalWorkflowRepository, ApprovalWorkflowRepository>();
        services.AddScoped<ITransactionLimitRepository, TransactionLimitRepository>();
        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddScoped<IKycRepository, KycRepository>();
        services.AddScoped<IBillInstitutionRepository, BillInstitutionRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        // Infrastructure Services
        services.AddScoped<IIbanGenerator, IbanGenerator>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        // Email Configuration
        if (configuration != null)
        {
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        }
        services.AddScoped<IEmailSender, MailKitEmailSender>();
        
        // HTTP Context Accessor (for AuditLogger) - will be registered by API layer
        // Infrastructure just uses IHttpContextAccessor interface
        
        return services;
    }
}

