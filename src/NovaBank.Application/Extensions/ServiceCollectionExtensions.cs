using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NovaBank.Application.Accounts;
using NovaBank.Application.Admin;
using NovaBank.Application.Common;
using NovaBank.Application.Customers;
using NovaBank.Application.Reports;
using NovaBank.Application.Transactions;
using NovaBank.Application.Transfers;

namespace NovaBank.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // CurrentUser (header-based, MVP seviyesinde)
        services.AddScoped<CurrentUser>();

        // Services
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<ICustomersService, CustomersService>();
        services.AddScoped<ITransactionsService, TransactionsService>();
        services.AddScoped<ITransfersService, TransfersService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IAdminService, AdminService>();

        // Validators
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}

