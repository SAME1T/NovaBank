using System.Security.Cryptography;
using System.Text;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Email;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Customers;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Services;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.Customers;

public class CustomersService : ICustomersService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IIbanGenerator _ibanGenerator;
    private readonly IAuditLogger _auditLogger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomersService(
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository,
        IIbanGenerator ibanGenerator,
        IAuditLogger auditLogger,
        IJwtTokenService jwtTokenService,
        IEmailSender emailSender,
        IPasswordResetRepository passwordResetRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _ibanGenerator = ibanGenerator;
        _auditLogger = auditLogger;
        _jwtTokenService = jwtTokenService;
        _emailSender = emailSender;
        _passwordResetRepository = passwordResetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<CustomerResponse>.Failure(ErrorCodes.Validation, "FirstName/LastName boş olamaz.");

        if (await _customerRepository.ExistsByTcknAsync(request.NationalId, ct))
            return Result<CustomerResponse>.Failure(ErrorCodes.Conflict, "NationalId zaten kayıtlı.");

        var customer = new Customer(
            new NationalId(request.NationalId),
            request.FirstName,
            request.LastName,
            request.Email ?? string.Empty,
            request.Phone ?? string.Empty,
            request.Password
        );

        await _customerRepository.AddAsync(customer, ct);

        // Otomatik vadesiz (TRY) hesap aç
        var rnd = new Random();
        long accountNo;
        do
        {
            accountNo = rnd.Next(100000, 999999);
        } while (await _accountRepository.ExistsByAccountNoAsync(accountNo, ct));

        // Benzersiz IBAN
        string iban;
        do
        {
            iban = _ibanGenerator.GenerateIban();
        } while (await _accountRepository.ExistsByIbanAsync(iban, ct));

        var newAccount = new Account(
            customer.Id,
            new AccountNo(accountNo),
            new Iban(iban),
            Currency.TRY,
            new Money(0m, Currency.TRY),
            0m
        );

        await _accountRepository.AddAsync(newAccount, ct);

        var response = new CustomerResponse(
            customer.Id,
            customer.NationalId.Value,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.IsActive
        );

        return Result<CustomerResponse>.Success(response);
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);
        if (customer is null)
            return Result<CustomerResponse>.Failure(ErrorCodes.NotFound, "Customer bulunamadı.");

        var response = new CustomerResponse(
            customer.Id,
            customer.NationalId.Value,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.IsActive
        );

        return Result<CustomerResponse>.Success(response);
    }

    public async Task<Result<List<CustomerResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await _customerRepository.GetAllAsync(ct);
        var responses = customers.Select(c => new CustomerResponse(
            c.Id,
            c.NationalId.Value,
            c.FirstName,
            c.LastName,
            c.Email,
            c.Phone,
            c.IsActive
        )).ToList();

        return Result<List<CustomerResponse>>.Success(responses);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByTcknAsync(request.NationalId, ct);
        if (customer is null)
        {
            await _auditLogger.LogAsync(
                AuditAction.LoginFailed.ToString(),
                success: false,
                entityType: "Customer",
                summary: "Kullanıcı bulunamadı",
                errorCode: ErrorCodes.NotFound,
                ct: ct);
            return Result<LoginResponse>.Failure(ErrorCodes.NotFound, "Kullanıcı bulunamadı.");
        }

        if (!customer.VerifyPassword(request.Password))
        {
            await _auditLogger.LogAsync(
                AuditAction.LoginFailed.ToString(),
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre hatalı",
                errorCode: ErrorCodes.Unauthorized,
                ct: ct);
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Şifre hatalı.");
        }

        if (!customer.IsActive)
        {
            await _auditLogger.LogAsync(
                AuditAction.LoginFailed.ToString(),
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Hesap deaktif",
                errorCode: ErrorCodes.Unauthorized,
                ct: ct);
            return Result<LoginResponse>.Failure(ErrorCodes.Unauthorized, "Hesap deaktif.");
        }

        var (token, expiresAt) = _jwtTokenService.CreateToken(customer.Id, customer.Role.ToString());
        var response = new LoginResponse(
            customer.Id,
            $"{customer.FirstName} {customer.LastName}",
            customer.Role,
            token,
            expiresAt
        );

        await _auditLogger.LogAsync(
            AuditAction.LoginSuccess.ToString(),
            success: true,
            entityType: "Customer",
            entityId: customer.Id.ToString(),
            summary: $"Login başarılı: {customer.FirstName} {customer.LastName}",
            ct: ct);

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<PasswordResetRequestResponse>> RequestPasswordResetAsync(PasswordResetRequest req, CancellationToken ct = default)
    {
        // Email veya NationalId ile customer bul
        var customer = await _customerRepository.FindByEmailOrNationalIdAsync(req.EmailOrNationalId, ct);

        // Bulamazsa bile 200 dön (güvenlik için)
        if (customer == null)
        {
            return Result<PasswordResetRequestResponse>.Success(
                new PasswordResetRequestResponse("Eğer hesap varsa kod gönderildi."));
        }

        // 6 haneli kod üret
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();
        var codeHash = HashCode(code);

        // Token oluştur (10 dakika geçerli)
        var expiresAt = DateTime.UtcNow.AddMinutes(10);
        var token = new PasswordResetToken(
            customer.Id,
            customer.Email,
            codeHash,
            expiresAt);

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
            {
                await _passwordResetRepository.AddAsync(token, transactionCt);
            }, ct);

            // Mail gönder (DB kaydı başarılı olduktan sonra)
            try
            {
                var subject = "NovaBank Şifre Sıfırlama Kodu";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Şifre Sıfırlama Kodu</h2>
                        <p>Merhaba {customer.FirstName},</p>
                        <p>Şifre sıfırlama kodunuz:</p>
                        <h1 style='color: #1976d2; font-size: 32px; letter-spacing: 4px;'>{code}</h1>
                        <p>Bu kod 10 dakika geçerlidir.</p>
                        <p>Eğer bu isteği siz yapmadıysanız, lütfen bu e-postayı görmezden gelin.</p>
                        <p>Saygılarımızla,<br/>NovaBank</p>
                    </body>
                    </html>";

                await _emailSender.SendAsync(customer.Email, subject, htmlBody, ct);

                // Audit log - başarılı (mail gönderildi)
                await _auditLogger.LogAsync(
                    "PasswordResetEmailSent",
                    success: true,
                    entityType: "Customer",
                    entityId: customer.Id.ToString(),
                    summary: $"Şifre sıfırlama kodu mail ile gönderildi: {customer.Email}",
                    ct: ct);

                return Result<PasswordResetRequestResponse>.Success(
                    new PasswordResetRequestResponse("Eğer hesap varsa kod gönderildi."));
            }
            catch (Exception emailEx)
            {
                // SMTP 535 hatası kontrolü
                var isSmtpAuthError = emailEx.Message.Contains("535") || 
                                     emailEx.Message.Contains("Authentication") ||
                                     emailEx.Message.Contains("Username and Password not accepted");
                
                // Summary kısa tutulmalı (256 char sınırı var, AuditLogger'da truncate edilir ama yine de kısaltıyoruz)
                var summaryMsg = isSmtpAuthError 
                    ? "E-posta gönderilemedi: SMTP kimlik doğrulama hatası (Gmail App Password gerekli)" 
                    : "E-posta gönderilemedi: SMTP hatası";
                
                if (summaryMsg.Length > 200)
                    summaryMsg = summaryMsg.Substring(0, 197) + "...";
                
                await _auditLogger.LogAsync(
                    "PasswordResetEmailFailed",
                    success: false,
                    entityType: "Customer",
                    entityId: customer.Id.ToString(),
                    summary: summaryMsg,
                    errorCode: ErrorCodes.EmailSendFailed,
                    ct: ct);

                // Kullanıcıya Türkçe mesaj
                var userFriendlyMsg = isSmtpAuthError
                    ? "Kod gönderilemedi: SMTP ayarlarını kontrol edin (Gmail App Password gerekli)."
                    : "Kod gönderilemedi: E-posta gönderim hatası. Lütfen daha sonra tekrar deneyin.";

                return Result<PasswordResetRequestResponse>.Failure(
                    ErrorCodes.EmailSendFailed,
                    userFriendlyMsg);
            }
        }
        catch (Exception ex)
        {
            // DB kaydı veya genel hata - audit log (summary kısa tutulmalı, 256 char sınırı var)
            var shortErrorMsg = ex.Message.Length > 150 
                ? ex.Message.Substring(0, 150) + "..." 
                : ex.Message;
            await _auditLogger.LogAsync(
                "PasswordResetRequested",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: $"Şifre sıfırlama hatası: {shortErrorMsg}",
                errorCode: ErrorCodes.EmailSendFailed,
                ct: ct);

            // Detaylı hata mesajı döndür (Development için)
            var errorDetail = $"PASSWORD_RESET_FAILED: {ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException != null)
                errorDetail += $" | Inner: {ex.InnerException.Message}";

            return Result<PasswordResetRequestResponse>.Failure(
                ErrorCodes.EmailSendFailed,
                errorDetail);
        }
    }

    public async Task<Result<PasswordResetVerifyResponse>> VerifyPasswordResetAsync(PasswordResetVerifyRequest req, CancellationToken ct = default)
    {
        // Email veya NationalId ile customer bul
        var customer = await _customerRepository.FindByEmailOrNationalIdAsync(req.EmailOrNationalId, ct);

        if (customer == null)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                summary: "Şifre sıfırlama doğrulama başarısız: Kullanıcı bulunamadı",
                errorCode: ErrorCodes.CustomerNotFound,
                ct: ct);
            return Result<PasswordResetVerifyResponse>.Failure(
                ErrorCodes.InvalidResetCode,
                "Kod hatalı veya süresi dolmuş.");
        }

        // Token bul
        var token = await _passwordResetRepository.GetLatestValidAsync(customer.Id, ct);
        if (token == null)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama doğrulama başarısız: Token bulunamadı veya süresi doldu",
                errorCode: ErrorCodes.ResetTokenNotFoundOrExpired,
                ct: ct);
            return Result<PasswordResetVerifyResponse>.Failure(
                ErrorCodes.ResetTokenNotFoundOrExpired,
                "Kod hatalı veya süresi dolmuş.");
        }

        // Attempt count kontrolü
        if (token.AttemptCount >= 5)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama doğrulama başarısız: Çok fazla deneme",
                errorCode: ErrorCodes.InvalidResetCode,
                ct: ct);
            return Result<PasswordResetVerifyResponse>.Failure(
                ErrorCodes.InvalidResetCode,
                "Çok fazla hatalı deneme. Lütfen yeni bir kod isteyin.");
        }

        // Kod kontrolü
        var codeHash = HashCode(req.Code);
        if (token.CodeHash != codeHash)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
            {
                token.IncrementAttempt();
                await _passwordResetRepository.UpdateAsync(token, transactionCt);
                return Task.CompletedTask;
            }, ct);

            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama doğrulama başarısız: Geçersiz kod",
                errorCode: ErrorCodes.InvalidResetCode,
                ct: ct);
            return Result<PasswordResetVerifyResponse>.Failure(
                ErrorCodes.InvalidResetCode,
                "Kod hatalı.");
        }

        // Audit log - kod doğrulama başarılı
        await _auditLogger.LogAsync(
            "PasswordResetVerified",
            success: true,
            entityType: "Customer",
            entityId: customer.Id.ToString(),
            summary: "Şifre sıfırlama kodu doğrulandı",
            ct: ct);

        return Result<PasswordResetVerifyResponse>.Success(
            new PasswordResetVerifyResponse("Kod doğrulandı. Yeni şifrenizi girebilirsiniz."));
    }

    public async Task<Result<PasswordResetCompleteResponse>> CompletePasswordResetAsync(PasswordResetCompleteRequest req, CancellationToken ct = default)
    {
        // Email veya NationalId ile customer bul
        var customer = await _customerRepository.FindByEmailOrNationalIdAsync(req.EmailOrNationalId, ct);

        if (customer == null)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                summary: "Şifre sıfırlama tamamlama başarısız: Kullanıcı bulunamadı",
                errorCode: ErrorCodes.CustomerNotFound,
                ct: ct);
            return Result<PasswordResetCompleteResponse>.Failure(
                ErrorCodes.InvalidResetCode,
                "Kod hatalı veya süresi dolmuş.");
        }

        // Token bul
        var token = await _passwordResetRepository.GetLatestValidAsync(customer.Id, ct);
        if (token == null)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama tamamlama başarısız: Token bulunamadı veya süresi doldu",
                errorCode: ErrorCodes.ResetTokenNotFoundOrExpired,
                ct: ct);
            return Result<PasswordResetCompleteResponse>.Failure(
                ErrorCodes.ResetTokenNotFoundOrExpired,
                "Kod hatalı veya süresi dolmuş.");
        }

        // Attempt count kontrolü
        if (token.AttemptCount >= 5)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama tamamlama başarısız: Çok fazla deneme",
                errorCode: ErrorCodes.InvalidResetCode,
                ct: ct);
            return Result<PasswordResetCompleteResponse>.Failure(
                ErrorCodes.InvalidResetCode,
                "Çok fazla hatalı deneme. Lütfen yeni bir kod isteyin.");
        }

        // Kod kontrolü
        var codeHash = HashCode(req.Code);
        if (token.CodeHash != codeHash)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
            {
                token.IncrementAttempt();
                await _passwordResetRepository.UpdateAsync(token, transactionCt);
                return Task.CompletedTask;
            }, ct);

            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama tamamlama başarısız: Geçersiz kod",
                errorCode: ErrorCodes.InvalidResetCode,
                ct: ct);
            return Result<PasswordResetCompleteResponse>.Failure(
                ErrorCodes.InvalidResetCode,
                "Kod hatalı.");
        }

        // Şifre güncelle
        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            customer.UpdatePassword(req.NewPassword);
            await _customerRepository.UpdateAsync(customer, transactionCt);

            token.MarkAsUsed();
            await _passwordResetRepository.UpdateAsync(token, transactionCt);
            
            return Result.Success();
        }, ct);

        if (!result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                "PasswordResetFailed",
                success: false,
                entityType: "Customer",
                entityId: customer.Id.ToString(),
                summary: "Şifre sıfırlama tamamlama başarısız: Veritabanı hatası",
                errorCode: result.ErrorCode ?? ErrorCodes.InvalidOperation,
                ct: ct);
            return Result<PasswordResetCompleteResponse>.Failure(
                result.ErrorCode ?? ErrorCodes.InvalidOperation,
                result.ErrorMessage ?? "Şifre güncellenemedi.");
        }

        // Audit log - başarılı
        await _auditLogger.LogAsync(
            "PasswordResetCompleted",
            success: true,
            entityType: "Customer",
            entityId: customer.Id.ToString(),
            summary: "Şifre başarıyla sıfırlandı",
            ct: ct);

        return Result<PasswordResetCompleteResponse>.Success(
            new PasswordResetCompleteResponse("Şifre başarıyla güncellendi."));
    }

    private static string HashCode(string code)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hashedBytes);
    }
}

