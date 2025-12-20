using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Services;
using NovaBank.Infrastructure.Persistence;
using NovaBank.Infrastructure.Persistence.Repositories;
using NovaBank.Infrastructure.Persistence.UnitOfWork;
using NovaBank.Infrastructure.Services;

namespace NovaBank.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
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
        
        // Infrastructure Services
        services.AddScoped<IIbanGenerator, IbanGenerator>();
        
        return services;
    }
}
