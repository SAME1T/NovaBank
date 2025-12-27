namespace NovaBank.Core.Enums;

/// <summary>
/// Limit uygulama kapsamı.
/// </summary>
public enum LimitScope
{
    /// <summary>Tüm sistem için geçerli</summary>
    Global = 0,
    /// <summary>Rol bazlı</summary>
    Role = 1,
    /// <summary>Müşteri bazlı</summary>
    Customer = 2,
    /// <summary>Hesap bazlı</summary>
    Account = 3
}
