using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaBank.Core.Services;
using NovaBank.Infrastructure.Persistence;
using NovaBank.Infrastructure.Services;

namespace NovaBank.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        services.AddDbContext<BankDbContext>(opt =>
            opt.UseNpgsql(connectionString, x => x.MigrationsAssembly(typeof(BankDbContext).Assembly.FullName)));
        services.AddScoped<IIbanGenerator, IbanGenerator>();
        return services;
    }
}
