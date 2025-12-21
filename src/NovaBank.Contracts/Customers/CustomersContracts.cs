using NovaBank.Core.Enums;

namespace NovaBank.Contracts.Customers;
public sealed record CreateCustomerRequest(string NationalId, string FirstName, string LastName, string? Email, string? Phone, string Password);
public sealed record LoginRequest(string NationalId, string Password);
public sealed record CustomerResponse(Guid Id, string NationalId, string FirstName, string LastName, string? Email, string? Phone, bool IsActive);
public sealed record LoginResponse(Guid CustomerId, string FullName, UserRole Role, string AccessToken, DateTime ExpiresAt);

public sealed record PasswordResetRequest(string EmailOrNationalId);
public sealed record PasswordResetRequestResponse(string Message);
public sealed record PasswordResetVerifyRequest(string EmailOrNationalId, string Code);
public sealed record PasswordResetVerifyResponse(string Message);
public sealed record PasswordResetCompleteRequest(string EmailOrNationalId, string Code, string NewPassword);
public sealed record PasswordResetCompleteResponse(string Message);

