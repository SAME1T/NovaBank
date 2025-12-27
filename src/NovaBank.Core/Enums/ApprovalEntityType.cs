namespace NovaBank.Core.Enums;

/// <summary>
/// Onay gerektiren varlık türleri.
/// </summary>
public enum ApprovalEntityType
{
    /// <summary>Hesap açma</summary>
    AccountOpening = 0,
    /// <summary>Para transferi</summary>
    Transfer = 1,
    /// <summary>Kredi başvurusu</summary>
    LoanApplication = 2,
    /// <summary>Kredi kartı başvurusu</summary>
    CreditCardApplication = 3,
    /// <summary>Limit değişikliği</summary>
    LimitChange = 4,
    /// <summary>Kart bloke kaldırma</summary>
    CardUnblock = 5,
    /// <summary>Müşteri kayıt onayı</summary>
    CustomerRegistration = 6
}
