namespace NovaBank.Core.Enums;

/// <summary>
/// Komisyon türleri.
/// </summary>
public enum CommissionType
{
    /// <summary>Banka içi transfer</summary>
    InternalTransfer = 0,
    /// <summary>EFT</summary>
    Eft = 1,
    /// <summary>SWIFT/Uluslararası transfer</summary>
    Swift = 2,
    /// <summary>Döviz çevirme</summary>
    CurrencyExchange = 3,
    /// <summary>Hesap işletim ücreti</summary>
    AccountMaintenance = 4,
    /// <summary>Kart yıllık ücreti</summary>
    CardAnnualFee = 5,
    /// <summary>ATM çekim</summary>
    AtmWithdrawal = 6,
    /// <summary>Fatura ödeme</summary>
    BillPayment = 7,
    /// <summary>Kendi hesaplar arası transfer</summary>
    OwnAccountTransfer = 8,
    /// <summary>Döviz alım komisyonu</summary>
    CurrencyBuy = 9,
    /// <summary>Döviz satım komisyonu</summary>
    CurrencySell = 10
}
