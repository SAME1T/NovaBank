using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Customers;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Services;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.Customers;

public class CustomersService : ICustomersService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IIbanGenerator _ibanGenerator;

    public CustomersService(
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository,
        IIbanGenerator ibanGenerator)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _ibanGenerator = ibanGenerator;
    }

    public async Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<CustomerResponse>.Failure(ErrorCodes.Validation, "FirstName/LastName boş olamaz.");

        if (await _customerRepository.ExistsByTcknAsync(request.NationalId, ct))
            return Result<CustomerResponse>.Failure(ErrorCodes.Conflict, "NationalId zaten kayıtlı.");

        var customer = new Customer(
            new NationalId(request.NationalId),
            request.FirstName,
            request.LastName,
            request.Email ?? string.Empty,
            request.Phone ?? string.Empty,
            request.Password
        );

        await _customerRepository.AddAsync(customer, ct);

        // Otomatik vadesiz (TRY) hesap aç
        var rnd = new Random();
        long accountNo;
        do
        {
            accountNo = rnd.Next(100000, 999999);
        } while (await _accountRepository.ExistsByAccountNoAsync(accountNo, ct));

        // Benzersiz IBAN
        string iban;
        do
        {
            iban = _ibanGenerator.GenerateIban();
        } while (await _accountRepository.ExistsByIbanAsync(iban, ct));

        var newAccount = new Account(
            customer.Id,
            new AccountNo(accountNo),
            new Iban(iban),
            Currency.TRY,
            new Money(0m, Currency.TRY),
            0m
        );

        await _accountRepository.AddAsync(newAccount, ct);

        var response = new CustomerResponse(
            customer.Id,
            customer.NationalId.Value,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.IsActive
        );

        return Result<CustomerResponse>.Success(response);
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);
        if (customer is null)
            return Result<CustomerResponse>.Failure(ErrorCodes.NotFound, "Customer bulunamadı.");

        var response = new CustomerResponse(
            customer.Id,
            customer.NationalId.Value,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.IsActive
        );

        return Result<CustomerResponse>.Success(response);
    }

    public async Task<Result<List<CustomerResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await _customerRepository.GetAllAsync(ct);
        var responses = customers.Select(c => new CustomerResponse(
            c.Id,
            c.NationalId.Value,
            c.FirstName,
            c.LastName,
            c.Email,
            c.Phone,
            c.IsActive
        )).ToList();

        return Result<List<CustomerResponse>>.Success(responses);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByTcknAsync(request.NationalId, ct);
        if (customer is null)
            return Result<LoginResponse>.Failure(ErrorCodes.NotFound, "Kullanıcı bulunamadı.");

        if (!customer.VerifyPassword(request.Password))
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Şifre hatalı.");

        if (!customer.IsActive)
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Hesap deaktif.");

        var response = new LoginResponse(
            customer.Id,
            $"{customer.FirstName} {customer.LastName}",
            customer.Role
        );

        return Result<LoginResponse>.Success(response);
    }
}

