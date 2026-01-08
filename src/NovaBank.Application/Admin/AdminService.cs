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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<List<CustomerSummaryResponse>>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<List<AccountAdminResponse>>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

        // Hesap kapatma (Closed) işlemi sadece Admin yapabilir
        if (status == AccountStatus.Closed && !_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Hesap kapatma işlemi sadece Admin tarafından yapılabilir.");

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
                    account.Activate(); // PendingApproval -> Active için de Activate çalışır
                    break;
                case AccountStatus.Frozen:
                    account.Freeze();
                    break;
                case AccountStatus.Closed:
                    account.Close();
                    break;
                case AccountStatus.PendingApproval:
                    // PendingApproval'a geri döndürülmez, sadece bilgi için
                    return Result.Failure(ErrorCodes.InvalidOperation, "Hesap 'Onay Bekliyor' durumuna geri döndürülemez.");
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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<UpdateCustomerActiveResponse>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");
        
        // Kullanıcı deaktive etme (silme benzeri) sadece Admin yapabilir
        if (!isActive && !_currentUser.IsAdmin)
            return Result<UpdateCustomerActiveResponse>.Failure(ErrorCodes.Unauthorized, "Kullanıcı deaktive etme işlemi sadece Admin tarafından yapılabilir.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<ResetCustomerPasswordResponse>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<List<AuditLogResponse>>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<List<PendingApprovalResponse>>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

        var responses = new List<PendingApprovalResponse>();

        // Müşteri onayları
        var customers = await _customerRepository.GetPendingApprovalsAsync(ct);
        responses.AddRange(customers.Select(c => new PendingApprovalResponse(
            c.Id,
            PendingItemType.Customer,
            $"{c.FirstName} {c.LastName}",
            c.NationalId.Value,
            c.Email ?? "",
            c.CreatedAt
        )));

        // Hesap onayları
        var accounts = await _accountRepository.GetPendingApprovalsAsync(ct);
        if (accounts.Count > 0)
        {
            // Customer bilgilerini toplu olarak al
            var customerIds = accounts.Select(a => a.CustomerId).Distinct().ToList();
            var accountCustomers = new Dictionary<Guid, Customer>();
            foreach (var customerId in customerIds)
            {
                var customer = await _customerRepository.GetByIdAsync(customerId, ct);
                if (customer != null)
                    accountCustomers[customerId] = customer;
            }

            // Hesap onaylarını response'a ekle
            responses.AddRange(accounts.Select(a =>
            {
                var customer = accountCustomers.GetValueOrDefault(a.CustomerId);
                if (customer == null)
                    return null;

                return new PendingApprovalResponse(
                    a.CustomerId,
                    PendingItemType.Account,
                    $"{customer.FirstName} {customer.LastName}",
                    customer.NationalId.Value,
                    customer.Email ?? "",
                    a.CreatedAt,
                    AccountId: a.Id,
                    Iban: a.Iban.Value,
                    Currency: a.Currency.ToString()
                );
            }).Where(r => r != null)!);
        }

        // Tarihe göre sırala
        var sortedResponses = responses.OrderBy(r => r.CreatedAt).ToList();
        return Result<List<PendingApprovalResponse>>.Success(sortedResponses);
    }

    public async Task<Result<ApproveCustomerResponse>> ApproveCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdminOrBranchManager)
            return Result<ApproveCustomerResponse>.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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
        if (!_currentUser.IsAdminOrBranchManager)
            return Result.Failure(ErrorCodes.Unauthorized, "Yönetici yetkisi gerekli.");

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

    public async Task<Result<CreateBranchManagerResponse>> CreateBranchManagerAsync(CreateBranchManagerRequest request, CancellationToken ct = default)
    {
        // BranchManager oluşturma sadece Admin yapabilir
        if (!_currentUser.IsAdmin)
            return Result<CreateBranchManagerResponse>.Failure(ErrorCodes.Unauthorized, "BranchManager oluşturma yetkisi sadece Admin'de vardır.");

        // Validasyonlar
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<CreateBranchManagerResponse>.Failure(ErrorCodes.Validation, "Ad/Soyad boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.NationalId) || request.NationalId.Length != 11)
            return Result<CreateBranchManagerResponse>.Failure(ErrorCodes.Validation, "TC Kimlik No 11 haneli olmalıdır.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<CreateBranchManagerResponse>.Failure(ErrorCodes.Validation, "E-posta adresi gereklidir.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result<CreateBranchManagerResponse>.Failure(ErrorCodes.Validation, "Şifre en az 6 karakter olmalıdır.");

        // TC Kimlik No zaten kayıtlı mı?
        if (await _customerRepository.ExistsByTcknAsync(request.NationalId, ct))
            return Result<CreateBranchManagerResponse>.Failure(ErrorCodes.Conflict, "Bu TC Kimlik No ile kayıtlı bir kullanıcı zaten var.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var customer = new Customer(
                new NovaBank.Core.ValueObjects.NationalId(request.NationalId),
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone ?? string.Empty,
                request.Password,
                UserRole.BranchManager // Direkt BranchManager rolü ile oluştur
            );

            // BranchManager otomatik olarak onaylı ve aktif
            customer.Approve();
            customer.Activate();

            await _customerRepository.AddAsync(customer, transactionCt);

            var response = new CreateBranchManagerResponse(
                customer.Id,
                customer.FullName,
                customer.Role.ToString()
            );

            return Result<CreateBranchManagerResponse>.Success(response);
        }, ct);

        if (result.IsSuccess && result.Value != null)
        {
            await _auditLogger.LogAsync(
                "BranchManagerCreated",
                success: true,
                entityType: "Customer",
                entityId: result.Value.CustomerId.ToString(),
                summary: $"Şube Bankacı Yöneticisi oluşturuldu: {result.Value.FullName}",
                metadata: new { customerId = result.Value.CustomerId, fullName = result.Value.FullName },
                ct: ct);
        }

        return result;
    }

    public async Task<Result<UpdateCustomerRoleResponse>> UpdateCustomerRoleAsync(Guid customerId, UserRole newRole, CancellationToken ct = default)
    {
        // Rol güncelleme sadece Admin yapabilir
        if (!_currentUser.IsAdmin)
            return Result<UpdateCustomerRoleResponse>.Failure(ErrorCodes.Unauthorized, "Rol güncelleme yetkisi sadece Admin'de vardır.");

        // Admin rolü atanamaz (güvenlik)
        if (newRole == UserRole.Admin)
            return Result<UpdateCustomerRoleResponse>.Failure(ErrorCodes.Unauthorized, "Admin rolü atanamaz.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, transactionCt);
            if (customer is null)
                return Result<UpdateCustomerRoleResponse>.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

            // Admin'in rolü değiştirilemez
            if (customer.Role == UserRole.Admin)
                return Result<UpdateCustomerRoleResponse>.Failure(ErrorCodes.Unauthorized, "Admin kullanıcısının rolü değiştirilemez.");

            var oldRole = customer.Role;
            customer.UpdateRole(newRole);
            await _customerRepository.UpdateAsync(customer, transactionCt);

            var response = new UpdateCustomerRoleResponse(customerId, newRole.ToString());
            return Result<UpdateCustomerRoleResponse>.Success(response);
        }, ct);

        if (result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                "CustomerRoleUpdated",
                success: true,
                entityType: "Customer",
                entityId: customerId.ToString(),
                summary: $"Kullanıcı rolü güncellendi: {newRole}",
                metadata: new { customerId, newRole = newRole.ToString() },
                ct: ct);
        }

        return result;
    }

    public async Task<Result> DeleteAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        // Hesap silme sadece Admin yapabilir
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Hesap silme yetkisi sadece Admin'de vardır.");

        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account is null)
            return Result.Failure(ErrorCodes.NotFound, "Hesap bulunamadı.");

        var iban = account.Iban.Value;
        var currency = account.Currency.ToString();
        var customerId = account.CustomerId;

        // Önce audit log kaydet (silmeden önce)
        await _auditLogger.LogAsync(
            "AccountDeleted",
            success: true,
            entityType: "Account",
            entityId: accountId.ToString(),
            summary: $"Hesap silindi - IBAN: {iban}, Para Birimi: {currency}",
            metadata: new { accountId, iban, currency, customerId },
            ct: ct);

        // Hesabı sil
        await _accountRepository.DeleteAsync(accountId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DeleteCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        // Müşteri silme sadece Admin yapabilir
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Müşteri silme yetkisi sadece Admin'de vardır.");

        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer is null)
            return Result.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

        // Admin kullanıcısı silinemez
        if (customer.Role == UserRole.Admin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin kullanıcısı silinemez.");

        var fullName = $"{customer.FirstName} {customer.LastName}";
        var nationalId = customer.NationalId.Value;

        // Önce müşterinin tüm hesaplarını sil
        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId, ct);
        foreach (var account in accounts)
        {
            await _auditLogger.LogAsync(
                "AccountDeleted",
                success: true,
                entityType: "Account",
                entityId: account.Id.ToString(),
                summary: $"Müşteri silinirken hesap silindi - IBAN: {account.Iban.Value}",
                metadata: new { accountId = account.Id, iban = account.Iban.Value, customerId },
                ct: ct);

            await _accountRepository.DeleteAsync(account.Id, ct);
        }

        // Audit log kaydet (müşteri silinmeden önce)
        await _auditLogger.LogAsync(
            "CustomerDeleted",
            success: true,
            entityType: "Customer",
            entityId: customerId.ToString(),
            summary: $"Müşteri silindi: {fullName} (TC: {nationalId})",
            metadata: new { customerId, fullName, nationalId, deletedAccountsCount = accounts.Count },
            ct: ct);

        // Müşteriyi sil
        await _customerRepository.DeleteAsync(customerId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

