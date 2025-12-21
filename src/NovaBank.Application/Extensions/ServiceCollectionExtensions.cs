using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NovaBank.Application.Accounts;
using NovaBank.Application.Admin;
using NovaBank.Application.Common;
using NovaBank.Application.Customers;
using NovaBank.Application.Reports;
using NovaBank.Application.Transactions;
using NovaBank.Application.Transfers;
using NovaBank.Core.Enums;
using System.Security.Claims;

namespace NovaBank.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // CurrentUser (JWT Claims-based)
        services.AddScoped<CurrentUser>(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var user = httpContextAccessor.HttpContext?.User;
            var cu = new CurrentUser();
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleStr = user.FindFirst(ClaimTypes.Role)?.Value;
                
                if (Guid.TryParse(sub, out var cid))
                    cu.CustomerId = cid;
                
                if (!string.IsNullOrWhiteSpace(roleStr) && Enum.TryParse<UserRole>(roleStr, out var role))
                    cu.Role = role;
            }
            
            return cu;
        });

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

