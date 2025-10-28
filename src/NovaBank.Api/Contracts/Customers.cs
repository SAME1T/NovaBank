namespace NovaBank.Api.Contracts;
public sealed record CreateCustomerRequest(string NationalId, string FirstName, string LastName, string? Email, string? Phone, string Password);
public sealed record LoginRequest(string NationalId, string Password);
public sealed record CustomerResponse(Guid Id, string NationalId, string FirstName, string LastName, string? Email, string? Phone, bool IsActive);
