using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.Common;

/// <summary>
/// Sistem hesapları için sabit değerler.
/// </summary>
public static class SystemAccounts
{
    /// <summary>Sistem kasa hesabı IBAN'ı (TRY).</summary>
    public const string CashTryIban = "TR00CASH000000000000000000";

    /// <summary>Sistem müşteri TCKN'ı.</summary>
    public const string SystemCustomerNationalId = "00000000000";

    /// <summary>Sistem müşteri adı.</summary>
    public const string SystemCustomerFirstName = "Sistem";

    /// <summary>Sistem müşteri soyadı.</summary>
    public const string SystemCustomerLastName = "Hesabı";
}

