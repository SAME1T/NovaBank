using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Persistence.Seeding;

/// <summary>
/// Sistem hesabı (kasa) seed işlemleri.
/// </summary>
public class SystemAccountSeeder
{
    private readonly BankDbContext _context;
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;

    public SystemAccountSeeder(
        BankDbContext context,
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository)
    {
        _context = context;
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Sistem müşterisi ve kasa hesabını oluşturur (idempotent).
    /// </summary>
    public async Task SeedSystemAccountsAsync(CancellationToken ct = default)
    {
        // Sistem müşterisini kontrol et veya oluştur
        var systemCustomer = await _customerRepository.GetByTcknAsync(SystemAccounts.SystemCustomerNationalId, ct);
        if (systemCustomer is null)
        {
            systemCustomer = new Customer(
                new NationalId(SystemAccounts.SystemCustomerNationalId),
                SystemAccounts.SystemCustomerFirstName,
                SystemAccounts.SystemCustomerLastName,
                "system@novabank.com",
                "",
                "SystemPassword123!" // Sistem şifresi (gerçek kullanımda güvenli saklanmalı)
            );
            await _customerRepository.AddAsync(systemCustomer, ct);
            await _context.SaveChangesAsync(ct); // İlk kayıt için SaveChanges gerekli
        }

        // Sistem kasa hesabını kontrol et veya oluştur
        var cashAccount = await _accountRepository.GetByIbanAsync(SystemAccounts.CashTryIban, ct);
        if (cashAccount is null)
        {
            cashAccount = new Account(
                systemCustomer.Id,
                new AccountNo(999999), // Sistem hesabı numarası
                new Iban(SystemAccounts.CashTryIban),
                Currency.TRY,
                new Money(0m, Currency.TRY),
                0m // Sistem hesabı overdraft limiti yok
            );
            await _accountRepository.AddAsync(cashAccount, ct);
            await _context.SaveChangesAsync(ct); // İlk kayıt için SaveChanges gerekli
        }
    }
}

