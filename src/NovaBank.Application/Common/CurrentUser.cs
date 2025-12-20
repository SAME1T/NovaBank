using NovaBank.Core.Enums;

namespace NovaBank.Application.Common;

/// <summary>
/// Mevcut kullanıcı bilgileri (header-based, MVP seviyesinde).
/// </summary>
public class CurrentUser
{
    public Guid? CustomerId { get; set; }
    public UserRole? Role { get; set; }

    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsCustomer => Role == UserRole.Customer;

    public bool CanAccessCustomer(Guid customerId)
    {
        if (IsAdmin) return true;
        return CustomerId == customerId;
    }

    public bool CanAccessAccount(Guid accountCustomerId)
    {
        if (IsAdmin) return true;
        return CustomerId == accountCustomerId;
    }
}

