namespace NovaBank.Core.Enums;

/// <summary>
/// KYC doğrulama türleri.
/// </summary>
public enum VerificationType
{
    /// <summary>Kimlik doğrulama</summary>
    Identity = 0,
    /// <summary>Adres doğrulama</summary>
    Address = 1,
    /// <summary>Telefon doğrulama</summary>
    Phone = 2,
    /// <summary>E-posta doğrulama</summary>
    Email = 3,
    /// <summary>Gelir doğrulama</summary>
    Income = 4
}
