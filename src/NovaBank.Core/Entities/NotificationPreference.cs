using NovaBank.Core.Abstractions;

namespace NovaBank.Core.Entities;

/// <summary>
/// Müşteri bildirim tercihleri.
/// </summary>
public sealed class NotificationPreference : Entity
{
    private NotificationPreference() { }

    public Guid CustomerId { get; private set; }
    
    // İşlem bildirimleri
    public bool TransactionSms { get; private set; } = true;
    public bool TransactionEmail { get; private set; } = true;
    
    // Giriş bildirimleri
    public bool LoginSms { get; private set; } = true;
    public bool LoginEmail { get; private set; } = true;
    
    // Pazarlama bildirimleri
    public bool MarketingSms { get; private set; } = false;
    public bool MarketingEmail { get; private set; } = false;
    
    // Güvenlik bildirimleri
    public bool SecurityAlertSms { get; private set; } = true;
    public bool SecurityAlertEmail { get; private set; } = true;

    public NotificationPreference(Guid customerId)
    {
        CustomerId = customerId;
    }

    public void UpdateTransactionPreferences(bool sms, bool email)
    {
        TransactionSms = sms;
        TransactionEmail = email;
        TouchUpdated();
    }

    public void UpdateLoginPreferences(bool sms, bool email)
    {
        LoginSms = sms;
        LoginEmail = email;
        TouchUpdated();
    }

    public void UpdateMarketingPreferences(bool sms, bool email)
    {
        MarketingSms = sms;
        MarketingEmail = email;
        TouchUpdated();
    }

    public void UpdateSecurityPreferences(bool sms, bool email)
    {
        SecurityAlertSms = sms;
        SecurityAlertEmail = email;
        TouchUpdated();
    }

    public void UpdateAll(
        bool transactionSms, bool transactionEmail,
        bool loginSms, bool loginEmail,
        bool marketingSms, bool marketingEmail,
        bool securitySms, bool securityEmail)
    {
        TransactionSms = transactionSms;
        TransactionEmail = transactionEmail;
        LoginSms = loginSms;
        LoginEmail = loginEmail;
        MarketingSms = marketingSms;
        MarketingEmail = marketingEmail;
        SecurityAlertSms = securitySms;
        SecurityAlertEmail = securityEmail;
        TouchUpdated();
    }
}
