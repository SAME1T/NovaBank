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
        const string AdminPassword = "Admin123!"; // TODO: Production'da güvenli şekilde saklanmalı

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
            await _customerRepository.AddAsync(admin, ct);
            await _context.SaveChangesAsync(ct); // İlk kayıt için SaveChanges gerekli
        }
        else if (admin.Role != UserRole.Admin)
        {
            // Eğer admin kullanıcısı varsa ama role'ü Admin değilse güncelle
            // Not: Customer entity'sinde Role setter yok, bu yüzden reflection veya yeni bir metod gerekebilir
            // Şimdilik sadece seed'de kontrol ediyoruz
        }
    }
}

