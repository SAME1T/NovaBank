using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.CreditCards;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Application.CreditCards;

public class CreditCardService : ICreditCardService
{
    private readonly ICreditCardApplicationRepository _applicationRepository;
    private readonly ICardRepository _cardRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;
    private readonly IAuditLogger _auditLogger;

    public CreditCardService(
        ICreditCardApplicationRepository applicationRepository,
        ICardRepository cardRepository,
        ICustomerRepository customerRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        CurrentUser currentUser,
        IAuditLogger auditLogger)
    {
        _applicationRepository = applicationRepository;
        _cardRepository = cardRepository;
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task<Result> ApplyForCreditCardAsync(ApplyCreditCardRequest req, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure(ErrorCodes.Unauthorized, "Giriş yapmanız gerekiyor.");

        var customer = await _customerRepository.GetByIdAsync(_currentUser.CustomerId!.Value, ct);
        if (customer == null)
            return Result.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

        if (!customer.IsApproved)
            return Result.Failure(ErrorCodes.Unauthorized, "Hesabınız henüz onaylanmadı.");

        // Bekleyen başvuru var mı kontrol et
        var existingPending = await _applicationRepository.HasPendingApplicationAsync(_currentUser.CustomerId.Value, ct);
        
        if (existingPending)
            return Result.Failure(ErrorCodes.Validation, "Zaten bekleyen bir başvurunuz var.");

        if (req.RequestedLimit <= 0 || req.MonthlyIncome <= 0)
            return Result.Failure(ErrorCodes.Validation, "Limit ve aylık gelir 0'dan büyük olmalı.");

        var application = new CreditCardApplication(_currentUser.CustomerId.Value, req.RequestedLimit, req.MonthlyIncome);
        
        await _applicationRepository.AddAsync(application, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _auditLogger.LogAsync(
            "CreditCardApplication",
            success: true,
            entityType: "CreditCardApplication",
            entityId: application.Id.ToString(),
            summary: $"Kredi kartı başvurusu yapıldı. Talep edilen limit: {req.RequestedLimit:N2} TL",
            metadata: new { req.RequestedLimit, req.MonthlyIncome },
            ct: ct);

        return Result.Success();
    }

    public async Task<Result<List<CreditCardSummaryResponse>>> GetMyCardsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<List<CreditCardSummaryResponse>>.Failure(ErrorCodes.Unauthorized, "Giriş yapmanız gerekiyor.");

        var cards = await _cardRepository.GetCreditCardsByCustomerIdAsync(_currentUser.CustomerId!.Value, ct);

        var responses = cards.Select(c => new CreditCardSummaryResponse(
            c.Id,
            c.MaskedPan,
            c.CreditLimit ?? 0,
            c.AvailableLimit ?? 0,
            c.CurrentDebt,
            c.MinPaymentDueDate,
            c.MinPaymentAmount,
            c.CardStatus.ToString(),
            c.ExpiryMonth,
            c.ExpiryYear
        )).ToList();

        return Result<List<CreditCardSummaryResponse>>.Success(responses);
    }

    public async Task<Result<List<MyApplicationResponse>>> GetMyApplicationsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<List<MyApplicationResponse>>.Failure(ErrorCodes.Unauthorized, "Giriş yapmanız gerekiyor.");

        var applications = await _applicationRepository.GetByCustomerIdAsync(_currentUser.CustomerId!.Value, ct);

        var responses = applications.Select(a => new MyApplicationResponse(
            a.Id,
            a.RequestedLimit,
            a.Status.ToString(),
            a.RejectionReason,
            a.CreatedAt,
            a.ProcessedAt
        )).ToList();

        return Result<List<MyApplicationResponse>>.Success(responses);
    }

    public async Task<Result> MakeCardPaymentAsync(Guid cardId, decimal amount, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure(ErrorCodes.Unauthorized, "Giriş yapmanız gerekiyor.");

        if (amount <= 0)
            return Result.Failure(ErrorCodes.Validation, "Ödeme tutarı 0'dan büyük olmalı.");

        var cards = await _cardRepository.GetByCustomerIdAsync(_currentUser.CustomerId!.Value, ct);
        var card = cards.FirstOrDefault(c => c.Id == cardId);
        
        if (card == null)
            return Result.Failure(ErrorCodes.NotFound, "Kart bulunamadı.");

        if (card.CurrentDebt <= 0)
            return Result.Failure(ErrorCodes.Validation, "Ödeme yapılacak borç bulunmuyor.");

        var paymentAmount = Math.Min(amount, card.CurrentDebt);
        card.MakePayment(paymentAmount);

        await _unitOfWork.SaveChangesAsync(ct);

        await _auditLogger.LogAsync(
            "CreditCardPayment",
            success: true,
            entityType: "Card",
            entityId: cardId.ToString(),
            summary: $"Kredi kartı ödemesi yapıldı: {paymentAmount:N2} TL. Kalan borç: {card.CurrentDebt:N2} TL",
            metadata: new { paymentAmount, remainingDebt = card.CurrentDebt },
            ct: ct);

        return Result.Success();
    }

    public async Task<Result<List<CreditCardApplicationResponse>>> GetPendingApplicationsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<List<CreditCardApplicationResponse>>.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        var applications = await _applicationRepository.GetPendingApplicationsAsync(ct);

        var responses = new List<CreditCardApplicationResponse>();
        foreach (var a in applications)
        {
            var customer = await _customerRepository.GetByIdAsync(a.CustomerId, ct);
            responses.Add(new CreditCardApplicationResponse(
                a.Id,
                a.CustomerId,
                customer != null ? $"{customer.FirstName} {customer.LastName}" : "Bilinmiyor",
                a.RequestedLimit,
                a.ApprovedLimit,
                a.MonthlyIncome,
                a.Status.ToString(),
                a.RejectionReason,
                a.CreatedAt,
                a.ProcessedAt
            ));
        }

        return Result<List<CreditCardApplicationResponse>>.Success(responses);
    }

    public async Task<Result> ApproveApplicationAsync(Guid applicationId, decimal approvedLimit, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        if (approvedLimit <= 0)
            return Result.Failure(ErrorCodes.Validation, "Onaylanan limit 0'dan büyük olmalı.");

        var application = await _applicationRepository.GetByIdAsync(applicationId, ct);
        if (application == null)
            return Result.Failure(ErrorCodes.NotFound, "Başvuru bulunamadı.");

        if (application.Status != ApplicationStatus.Pending)
            return Result.Failure(ErrorCodes.Validation, "Sadece bekleyen başvurular onaylanabilir.");

        var customer = await _customerRepository.GetByIdAsync(application.CustomerId, ct);
        if (customer == null)
            return Result.Failure(ErrorCodes.NotFound, "Müşteri bulunamadı.");

        // Müşterinin TRY hesabını bul
        var accounts = await _accountRepository.GetByCustomerIdAsync(application.CustomerId, ct);
        var tryAccount = accounts.FirstOrDefault(a => a.Currency == Currency.TRY);
        
        if (tryAccount == null)
            return Result.Failure(ErrorCodes.Validation, "Müşterinin TRY hesabı bulunamadı. Önce hesap oluşturulmalı.");

        // Başvuruyu onayla
        application.Approve(approvedLimit, _currentUser.CustomerId!.Value);

        // Kredi kartı oluştur
        var card = Card.CreateCreditCard(application.CustomerId, tryAccount.Id, approvedLimit);
        await _cardRepository.AddAsync(card, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        await _auditLogger.LogAsync(
            "CreditCardApplicationApproved",
            success: true,
            entityType: "CreditCardApplication",
            entityId: applicationId.ToString(),
            summary: $"Kredi kartı başvurusu onaylandı. Limit: {approvedLimit:N2} TL",
            metadata: new { applicationId, approvedLimit, cardId = card.Id },
            ct: ct);

        return Result.Success();
    }

    public async Task<Result> RejectApplicationAsync(Guid applicationId, string reason, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Admin yetkisi gerekli.");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(ErrorCodes.Validation, "Red nedeni gerekli.");

        var application = await _applicationRepository.GetByIdAsync(applicationId, ct);
        if (application == null)
            return Result.Failure(ErrorCodes.NotFound, "Başvuru bulunamadı.");

        if (application.Status != ApplicationStatus.Pending)
            return Result.Failure(ErrorCodes.Validation, "Sadece bekleyen başvurular reddedilebilir.");

        application.Reject(reason, _currentUser.CustomerId!.Value);

        await _unitOfWork.SaveChangesAsync(ct);

        await _auditLogger.LogAsync(
            "CreditCardApplicationRejected",
            success: true,
            entityType: "CreditCardApplication",
            entityId: applicationId.ToString(),
            summary: $"Kredi kartı başvurusu reddedildi. Neden: {reason}",
            metadata: new { applicationId, reason },
            ct: ct);

        return Result.Success();
    }

    public async Task<Result> ProcessInterestsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure(ErrorCodes.Unauthorized, "Sadece yöneticiler faiz işletebilir.");

        var cards = await _cardRepository.GetAllAsync(ct);
        var creditCards = cards.Where(c => c.CardType == CardType.Credit && c.CurrentDebt > 0).ToList();
        int processedCount = 0;

        foreach (var card in creditCards)
        {
            // Eğer son ödeme tarihi geçmişse faizi işlet
            if (card.MinPaymentDueDate.HasValue && DateTime.UtcNow > card.MinPaymentDueDate.Value)
            {
                card.ApplyInterest(); // Varsayılan %5
                processedCount++;
            }
        }

        if (processedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLogger.LogAsync("InterestProcessed", true, "System", null, $"{processedCount} karta faiz uygulandı.");
        }

        return Result.Success();
    }
}
