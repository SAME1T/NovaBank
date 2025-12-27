namespace NovaBank.Core.Enums;

/// <summary>
/// İşlem limit türleri.
/// </summary>
public enum LimitType
{
    /// <summary>Günlük transfer limiti</summary>
    DailyTransfer = 0,
    /// <summary>Aylık transfer limiti</summary>
    MonthlyTransfer = 1,
    /// <summary>Tek seferlik işlem limiti</summary>
    SingleTransaction = 2,
    /// <summary>Günlük ATM çekim limiti</summary>
    DailyAtm = 3,
    /// <summary>Günlük POS işlem limiti</summary>
    DailyPos = 4
}
