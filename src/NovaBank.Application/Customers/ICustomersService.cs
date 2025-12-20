using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Customers;

namespace NovaBank.Application.Customers;

public interface ICustomersService
{
    Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<Result<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<CustomerResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

