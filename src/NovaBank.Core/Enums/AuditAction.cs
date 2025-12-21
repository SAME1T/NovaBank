namespace NovaBank.Core.Enums;

/// <summary>
/// Audit log aksiyonları.
/// </summary>
public enum AuditAction
{
    LoginSuccess,
    LoginFailed,
    Deposit,
    Withdraw,
    TransferInternal,
    TransferExternal,
    AdminUpdateOverdraft,
    AdminUpdateAccountStatus,
    AdminUpdateCustomerActive,
    AdminResetCustomerPassword
}

