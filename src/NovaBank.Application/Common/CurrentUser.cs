using NovaBank.Core.Enums;

namespace NovaBank.Application.Common;

/// <summary>
/// Mevcut kullanıcı bilgileri (header-based, MVP seviyesinde).
/// </summary>
public class CurrentUser
{
    public Guid? CustomerId { get; set; }
    public UserRole? Role { get; set; }

    public bool IsAuthenticated => CustomerId.HasValue;
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsManager => Role == UserRole.Manager;
    public bool IsCustomer => Role == UserRole.Customer;
    public bool IsBranchManager => Role == UserRole.BranchManager;
    
    /// <summary>
    /// Admin veya BranchManager yetkisi var mı? (Yönetim paneli erişimi için)
    /// </summary>
    public bool IsAdminOrBranchManager => Role == UserRole.Admin || Role == UserRole.BranchManager;

    public bool CanAccessCustomer(Guid customerId)
    {
        if (IsAdminOrBranchManager) return true;
        return CustomerId == customerId;
    }

    public bool CanAccessAccount(Guid accountCustomerId)
    {
        if (IsAdminOrBranchManager) return true;
        return CustomerId == accountCustomerId;
    }
}

