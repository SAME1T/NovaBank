namespace NovaBank.Core.Enums;

/// <summary>
/// Hesap türleri.
/// </summary>
public enum AccountType
{
    /// <summary>Vadesiz hesap</summary>
    Checking = 0,
    /// <summary>Vadeli/Tasarruf hesabı</summary>
    Savings = 1,
    /// <summary>Yatırım hesabı</summary>
    Investment = 2
}
