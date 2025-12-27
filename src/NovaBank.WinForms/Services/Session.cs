using NovaBank.Core.Enums;

namespace NovaBank.WinForms.Services;

/// <summary>
/// Uygulama session bilgilerini tutar.
/// </summary>
public static class Session
{
    public static Guid? CurrentCustomerId { get; set; }
    public static string? CurrentCustomerName { get; set; }
    public static Guid? SelectedAccountId { get; set; }
    public static UserRole? CurrentRole { get; set; }
    public static string? AccessToken { get; set; }

    public static bool IsAdmin => CurrentRole == UserRole.Admin;
    public static bool IsManager => CurrentRole == UserRole.Manager;

    public static void Clear()
    {
        CurrentCustomerId = null;
        CurrentCustomerName = null;
        SelectedAccountId = null;
        CurrentRole = null;
        AccessToken = null;
    }
}

