using System.Linq;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Email;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Admin;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Admin;

public class AdminService : IAdminService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;
    private readonly IAuditLogger _auditLogger;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEmailSender _emailSender;

    public AdminService(
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        CurrentUser currentUser,
        IAuditLogger auditLogger,
        IAuditLogRepository auditLogRepository,
        IEmailSender emailSender)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
        _auditLogRepository = auditLogRepository;
        _emailSender = emailSender;
    }

    public async Task<Result<List<CustomerSummaryResponse>>> SearchCustomersAsync(string? searchTerm, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<CustomerSummaryResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var customers = await _customerRepository.SearchAsync(searchTerm, ct);
        var responses = customers.Select(c =>
        {
            var nationalId = c.NationalId.Value;
            var masked = nationalId.Length == 11 
                ? $"{nationalId[..6]}****{nationalId[^1]}" 
                : nationalId;
            return new CustomerSummaryResponse(
                c.Id,
                $"{c.FirstName} {c.LastName}",
                masked,
                c.Role.ToString(),
                c.IsActive,
                c.IsApproved
            );
        }).ToList();

        return Result<List<CustomerSummaryResponse>>.Success(responses);
    }

    public async Task<Result<List<AccountAdminResponse>>> GetCustomerAccountsAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<AccountAdminResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId, ct);
        var responses = accounts.Select(a => new AccountAdminResponse(
            a.Id,
            a.Iban.Value,
            a.Currency.ToString(),
            a.Balance.Amount,
            a.OverdraftLimit,
            a.Status.ToString()
        )).ToList();

        return Result<List<AccountAdminResponse>>.Success(responses);
    }

    public async Task<Result> UpdateOverdraftLimitAsync(Guid accountId, decimal overdraftLimit, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        if (overdraftLimit < 0)
            return Result.Failure(ErrorCodes.Validation, "Overdraft limit negatif olamaz.");

        await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var account = await _accountRepository.GetByIdForUpdateAsync(accountId, transactionCt);
            if (account is null)
                throw new InvalidOperationException(ErrorCodes.AccountNotFound);

            account.UpdateOverdraftLimit(overdraftLimit);
            await _accountRepository.UpdateAsync(account, transactionCt);
        }, ct);

        // Audit log
        await _auditLogger.LogAsync(
            AuditAction.AdminUpdateOverdraft.ToString(),
            success: true,
            entityType: "Account",
            entityId: accountId.ToString(),
            summary: $"Overdraft limit güncellendi: {overdraftLimit}",
            metadata: new { accountId, overdraftLimit },
            ct: ct);

        return Result.Success();
    }

    public async Task<Result> UpdateAccountStatusAsync(Guid accountId, AccountStatus status, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var account = await _accountRepository.GetByIdForUpdateAsync(accountId, transactionCt);
            if (account is null)
                return Result.Failure(ErrorCodes.AccountNotFound, "Hesap bulunamadı.");

            // Kapalı hesap tekrar açılamaz
            if (account.Status == AccountStatus.Closed && status != AccountStatus.Closed)
                return Result.Failure(ErrorCodes.KapaliHesapTekrarAcilamaz, "Kapalı hesap tekrar açılamaz.");

            // Hesabı kapatmak için bakiye 0 olmalı
            if (status == AccountStatus.Closed && account.Balance.Amount != 0)
                return Result.Failure(ErrorCodes.KapatmakIcinBakiyeSifirOlmali, "Hesabı kapatmak için bakiye sıfır olmalı.");

            // Durumu güncelle
            switch (status)
            {
                case AccountStatus.Active:
                    account.Activate();
                    break;
                case AccountStatus.Frozen:
                    account.Freeze();
                    break;
                case AccountStatus.Closed:
                    account.Close();
                    break;
            }

            await _accountRepository.UpdateAsync(account, transactionCt);
            return Result.Success();
        }, ct);

        if (result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                AuditAction.AdminUpdateAccountStatus.ToString(),
                success: true,
                entityType: "Account",
                entityId: accountId.ToString(),
                summary: $"Hesap durumu güncellendi: {status}",
                metadata: new { accountId, status = status.ToString() },
                ct: ct);
        }
        else
        {
            await _auditLogger.LogAsync(
                AuditAction.AdminUpdateAccountStatus.ToString(),
                success: false,
                entityType: "Account",
                entityId: accountId.ToString(),
                summary: result.ErrorMessage ?? "Hesap durumu güncellenemedi",
                errorCode: result.ErrorCode,
                ct: ct);
        }

        return result;
    }

    public async Task<Result<UpdateCustomerActiveResponse>> UpdateCustomerActiveAsync(Guid customerId, bool isActive, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<UpdateCustomerActiveResponse>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, transactionCt);
            if (customer is null)
                return Result<UpdateCustomerActiveResponse>.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

            if (isActive)
                customer.Activate();
            else
                customer.Deactivate();

            await _customerRepository.UpdateAsync(customer, transactionCt);

            var response = new UpdateCustomerActiveResponse(customerId, customer.IsActive);
            return Result<UpdateCustomerActiveResponse>.Success(response);
        }, ct);

        if (result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                AuditAction.AdminUpdateCustomerActive.ToString(),
                success: true,
                entityType: "Customer",
                entityId: customerId.ToString(),
                summary: $"Müşteri aktiflik durumu güncellendi: {(isActive ? "Aktif" : "Pasif")}",
                metadata: new { customerId, isActive },
                ct: ct);
        }

        return result;
    }

    public async Task<Result<ResetCustomerPasswordResponse>> ResetCustomerPasswordAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<ResetCustomerPasswordResponse>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, transactionCt);
            if (customer is null)
                return Result<ResetCustomerPasswordResponse>.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

            // Geçici şifre üret: 12-14 karakter (büyük/küçük harf + rakam)
            var random = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789"; // I, O, 0, 1, l gibi karışık karakterler hariç
            var length = random.Next(12, 15);
            var temporaryPassword = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Şifreyi hash'le ve güncelle (Customer entity'nin UpdatePassword metodu kullanılır)
            customer.UpdatePassword(temporaryPassword);

            await _customerRepository.UpdateAsync(customer, transactionCt);

            var response = new ResetCustomerPasswordResponse(customerId, temporaryPassword);
            return Result<ResetCustomerPasswordResponse>.Success(response);
        }, ct);

        if (result.IsSuccess && result.Value != null)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, ct);
            if (customer != null)
            {
                try
                {
                    // Mail gönder
                    var subject = "NovaBank Geçici Şifre";
                    var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Geçici Şifre</h2>
                            <p>Merhaba {customer.FirstName},</p>
                            <p>Hesabınız için yeni bir geçici şifre oluşturuldu:</p>
                            <h1 style='color: #1976d2; font-size: 24px; letter-spacing: 2px;'>{result.Value.TemporaryPassword}</h1>
                            <p><strong>Lütfen ilk girişinizde bu şifreyi değiştirin.</strong></p>
                            <p>Saygılarımızla,<br/>NovaBank</p>
                        </body>
                        </html>";

                    await _emailSender.SendAsync(customer.Email, subject, htmlBody, ct);

                    await _auditLogger.LogAsync(
                        AuditAction.AdminResetCustomerPassword.ToString(),
                        success: true,
                        entityType: "Customer",
                        entityId: customerId.ToString(),
                        summary: "Müşteri şifresi sıfırlandı ve e-posta gönderildi",
                        metadata: new { customerId, resetCompleted = true, emailSent = true },
                        ct: ct);
                }
                catch (Exception ex)
                {
                    // Summary kısa tutulmalı, 256 char sınırı var (AuditLogger'da truncate edilir ama yine de kısaltıyoruz)
                    var shortErrorMsg = ex.Message.Length > 150 
                        ? ex.Message.Substring(0, 150) + "..." 
                        : ex.Message;
                    await _auditLogger.LogAsync(
                        AuditAction.AdminResetCustomerPassword.ToString(),
                        success: false,
                        entityType: "Customer",
                        entityId: customerId.ToString(),
                        summary: $"E-posta gönderilemedi: {shortErrorMsg}",
                        errorCode: ErrorCodes.EmailSendFailed,
                        metadata: new { customerId, resetCompleted = true, emailSent = false },
                        ct: ct);
                }
            }
        }

        return result;
    }

    public async Task<Result<List<AuditLogResponse>>> GetAuditLogsAsync(DateTime? from, DateTime? to, string? search, string? action, bool? success, int take, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<AuditLogResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var logs = await _auditLogRepository.QueryAsync(from, to, search, action, success, take, ct);

        var responses = logs.Select(log => new AuditLogResponse(
            log.Id,
            log.CreatedAt,
            log.ActorCustomerId,
            log.ActorRole,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.Success,
            log.ErrorCode,
            log.Summary
        )).ToList();

        return Result<List<AuditLogResponse>>.Success(responses);
    }

    public async Task<Result<List<PendingApprovalResponse>>> GetPendingApprovalsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<PendingApprovalResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var customers = await _customerRepository.GetPendingApprovalsAsync(ct);
        var responses = customers.Select(c => new PendingApprovalResponse(
            c.Id,
            $"{c.FirstName} {c.LastName}",
            c.NationalId.Value,
            c.Email ?? "",
            c.CreatedAt
        )).ToList();

        return Result<List<PendingApprovalResponse>>.Success(responses);
    }

    public async Task<Result<ApproveCustomerResponse>> ApproveCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<ApproveCustomerResponse>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, transactionCt);
            if (customer is null)
                return Result<ApproveCustomerResponse>.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

            customer.Approve();
            await _customerRepository.UpdateAsync(customer, transactionCt);

            var response = new ApproveCustomerResponse(customerId, customer.IsApproved);
            return Result<ApproveCustomerResponse>.Success(response);
        }, ct);

        if (result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                "CustomerApproved",
                success: true,
                entityType: "Customer",
                entityId: customerId.ToString(),
                summary: "Müşteri hesabı onaylandı",
                metadata: new { customerId },
                ct: ct);
        }

        return result;
    }

    public async Task<Result> RejectCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, transactionCt);
            if (customer is null)
                return Result.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

            customer.Reject();
            await _customerRepository.UpdateAsync(customer, transactionCt);

            return Result.Success();
        }, ct);

        if (result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                "CustomerRejected",
                success: true,
                entityType: "Customer",
                entityId: customerId.ToString(),
                summary: "Müşteri hesabı reddedildi ve pasif yapıldı",
                metadata: new { customerId },
                ct: ct);
        }

        return result;
    }
}

