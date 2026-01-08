using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Persistence.Seeding;

/// <summary>
/// Admin kullanıcı seed işlemleri.
/// </summary>
public class AdminSeeder
{
    private readonly BankDbContext _context;
    private readonly ICustomerRepository _customerRepository;

    public AdminSeeder(
        BankDbContext context,
        ICustomerRepository customerRepository)
    {
        _context = context;
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Admin kullanıcısını oluşturur (idempotent).
    /// </summary>
    public async Task SeedAdminAsync(CancellationToken ct = default)
    {
        const string AdminNationalId = "11111111111";
        const string AdminPassword = "123456";

        var admin = await _customerRepository.GetByTcknAsync(AdminNationalId, ct);
        if (admin is null)
        {
            admin = new Customer(
                new NationalId(AdminNationalId),
                "Admin",
                "User",
                "admin@novabank.com",
                "",
                AdminPassword,
                UserRole.Admin
            );
            admin.Approve(); // Admin otomatik onaylı olmalı
            await _customerRepository.AddAsync(admin, ct);
            await _context.SaveChangesAsync(ct); // İlk kayıt için SaveChanges gerekli
        }
        else
        {
            // Mevcut admin'in şifresini güncelle
            admin.UpdatePassword(AdminPassword);
            await _context.SaveChangesAsync(ct);
        }
    }
}

