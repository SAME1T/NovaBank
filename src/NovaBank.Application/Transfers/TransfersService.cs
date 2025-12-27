using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Transactions;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.Transfers;

public class TransfersService : ITransfersService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;
    private readonly CurrentUser _currentUser;

    public TransfersService(
        IAccountRepository accountRepository,
        ITransferRepository transferRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IAuditLogger auditLogger,
        CurrentUser currentUser)
    {
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _currentUser = currentUser;
    }

    public async Task<Result<TransferResponse>> TransferInternalAsync(TransferInternalRequest request, CancellationToken ct = default)
    {
        if (request.FromAccountId == request.ToAccountId)
        {
            await _auditLogger.LogAsync(
                AuditAction.TransferInternal.ToString(),
                success: false,
                entityType: "Transfer",
                summary: "Aynı hesaba transfer denemesi",
                errorCode: ErrorCodes.SameAccountTransfer,
                ct: ct);
            return Result<TransferResponse>.Failure(ErrorCodes.SameAccountTransfer, "Aynı hesaba transfer olmaz.");
        }

        if (request.Amount <= 0)
        {
            await _auditLogger.LogAsync(
                AuditAction.TransferInternal.ToString(),
                success: false,
                entityType: "Transfer",
                summary: "Geçersiz tutar",
                errorCode: ErrorCodes.InvalidAmount,
                ct: ct);
            return Result<TransferResponse>.Failure(ErrorCodes.InvalidAmount, "Tutar pozitif olmalı.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            // FOR UPDATE ile hesapları kilitle
            var fromAccount = await _accountRepository.GetByIdForUpdateAsync(request.FromAccountId, cancellationToken);
            var toAccount = await _accountRepository.GetByIdForUpdateAsync(request.ToAccountId, cancellationToken);

            if (fromAccount is null || toAccount is null)
            {
                await _auditLogger.LogAsync(
                    AuditAction.TransferInternal.ToString(),
                    success: false,
                    entityType: "Transfer",
                    summary: "Gönderen veya alıcı hesap bulunamadı",
                    errorCode: ErrorCodes.AccountNotFound,
                    metadata: new { fromAccountId = request.FromAccountId, toAccountId = request.ToAccountId },
                    ct: cancellationToken);
                return Result<TransferResponse>.Failure(ErrorCodes.AccountNotFound, "Gönderen veya alıcı hesap bulunamadı.");
            }

            // Hesap durumu kontrolleri
            if (fromAccount.Status == AccountStatus.Closed)
            {
                await _auditLogger.LogAsync(
                    AuditAction.TransferInternal.ToString(),
                    success: false,
                    entityType: "Account",
                    entityId: fromAccount.Id.ToString(),
                    summary: "Gönderen hesap kapalı",
                    errorCode: ErrorCodes.HesapKapali,
                    metadata: new { fromAccountId = request.FromAccountId },
                    ct: cancellationToken);
                return Result<TransferResponse>.Failure(ErrorCodes.HesapKapali, "Gönderen hesap kapalı.");
            }
            if (toAccount.Status == AccountStatus.Closed)
            {
                await _auditLogger.LogAsync(
                    AuditAction.TransferInternal.ToString(),
                    success: false,
                    entityType: "Account",
                    entityId: toAccount.Id.ToString(),
                    summary: "Alıcı hesap kapalı",
                    errorCode: ErrorCodes.HesapKapali,
                    metadata: new { toAccountId = request.ToAccountId },
                    ct: cancellationToken);
                return Result<TransferResponse>.Failure(ErrorCodes.HesapKapali, "Alıcı hesap kapalı.");
            }
            if (fromAccount.Status == AccountStatus.Frozen)
            {
                await _auditLogger.LogAsync(
                    AuditAction.TransferInternal.ToString(),
                    success: false,
                    entityType: "Account",
                    entityId: fromAccount.Id.ToString(),
                    summary: "Gönderen hesap dondurulmuş",
                    errorCode: ErrorCodes.HesapDondurulmus,
                    metadata: new { fromAccountId = request.FromAccountId },
                    ct: cancellationToken);
                return Result<TransferResponse>.Failure(ErrorCodes.HesapDondurulmus, "Gönderen hesap dondurulmuş.");
            }
            // Alıcı hesap Frozen olabilir (gelen para serbest)

            if (fromAccount.Currency != toAccount.Currency || fromAccount.Currency != request.Currency)
            {
                await _auditLogger.LogAsync(
                    AuditAction.TransferInternal.ToString(),
                    success: false,
                    entityType: "Transfer",
                    summary: "Para birimi uyuşmuyor",
                    errorCode: ErrorCodes.CurrencyMismatch,
                    ct: cancellationToken);
                return Result<TransferResponse>.Failure(ErrorCodes.CurrencyMismatch, "Para birimi uyuşmuyor.");
            }

            // Sistem kasa hesabı için bakiye kontrolü yapma (sonsuz para kaynağı)
            bool isSystemCashAccount = fromAccount.Iban.Value == SystemAccounts.CashTryIban;
            
            if (!isSystemCashAccount && !fromAccount.CanWithdraw(new Money(request.Amount, request.Currency)))
            {
                await _auditLogger.LogAsync(
                    AuditAction.TransferInternal.ToString(),
                    success: false,
                    entityType: "Account",
                    entityId: fromAccount.Id.ToString(),
                    summary: "Yetersiz bakiye/limit",
                    errorCode: ErrorCodes.InsufficientFunds,
                    metadata: new { fromAccountId = request.FromAccountId, amount = request.Amount },
                    ct: cancellationToken);
                return Result<TransferResponse>.Failure(ErrorCodes.InsufficientFunds, "Yetersiz bakiye/limit.");
            }

            // Bakiye güncellemeleri
            fromAccount.Withdraw(new Money(request.Amount, request.Currency));
            toAccount.Deposit(new Money(request.Amount, request.Currency));

            // Transfer kaydı
            var transfer = new Transfer(
                fromAccount.Id,
                toAccount.Id,
                new Money(request.Amount, request.Currency),
                TransferChannel.Internal
            );
            await _transferRepository.AddAsync(transfer, cancellationToken);

            // Çift kayıt: Debit ve Credit transaction'ları
            var referenceCode = Guid.NewGuid().ToString("N");
            var description = request.Description ?? string.Empty;

            var fromTransaction = new Transaction(
                fromAccount.Id,
                new Money(request.Amount, request.Currency),
                TransactionDirection.Debit,
                description
            );

            var toTransaction = new Transaction(
                toAccount.Id,
                new Money(request.Amount, request.Currency),
                TransactionDirection.Credit,
                description
            );

            // ReferenceCode'u aynı yap (aynı transfer işlemini gösterir)
            // Transaction entity'sinde ReferenceCode constructor'da otomatik oluşturuluyor
            // Aynı reference code için reflection veya entity'yi güncellemek gerekebilir
            // Şimdilik aynı description ile yeterli

            await _transactionRepository.AddAsync(fromTransaction, cancellationToken);
            await _transactionRepository.AddAsync(toTransaction, cancellationToken);

            // Hesapları güncelle
            await _accountRepository.UpdateAsync(fromAccount, cancellationToken);
            await _accountRepository.UpdateAsync(toAccount, cancellationToken);

            var response = new TransferResponse(
                transfer.Id,
                transfer.FromAccountId,
                transfer.ToAccountId ?? Guid.Empty,
                transfer.Amount.Amount,
                transfer.Amount.Currency.ToString(),
                transfer.Channel.ToString(),
                transfer.Status.ToString(),
                transfer.CreatedAt
            );

            return Result<TransferResponse>.Success(response);
        }, ct);

        // Audit log - başarılı transfer
        if (result.IsSuccess && result.Value != null)
        {
            await _auditLogger.LogAsync(
                AuditAction.TransferInternal.ToString(),
                success: true,
                entityType: "Transfer",
                entityId: result.Value.Id.ToString(),
                summary: $"İç transfer: {request.FromAccountId} -> {request.ToAccountId}",
                metadata: new { 
                    transferId = result.Value.Id,
                    fromAccountId = request.FromAccountId,
                    toAccountId = request.ToAccountId,
                    amount = request.Amount,
                    currency = request.Currency.ToString()
                },
                ct: ct);
        }
        else if (!result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                AuditAction.TransferInternal.ToString(),
                success: false,
                entityType: "Transfer",
                summary: result.ErrorMessage ?? "Transfer başarısız",
                errorCode: result.ErrorCode,
                metadata: new { fromAccountId = request.FromAccountId, toAccountId = request.ToAccountId },
                ct: ct);
        }

        return result;
    }

    public async Task<Result<TransferResponse>> TransferExternalAsync(TransferExternalRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return Result<TransferResponse>.Failure(ErrorCodes.InvalidAmount, "Tutar pozitif olmalı.");

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            // FOR UPDATE ile gönderen hesabı kilitle
            var fromAccount = await _accountRepository.GetByIdForUpdateAsync(request.FromAccountId, cancellationToken);
            if (fromAccount is null)
                return Result<TransferResponse>.Failure(ErrorCodes.AccountNotFound, "Gönderen hesap bulunamadı.");

            // Hesap durumu kontrolleri - gönderen
            if (fromAccount.Status == AccountStatus.Closed)
                return Result<TransferResponse>.Failure(ErrorCodes.HesapKapali, "Gönderen hesap kapalı.");
            if (fromAccount.Status == AccountStatus.Frozen)
                return Result<TransferResponse>.Failure(ErrorCodes.HesapDondurulmus, "Gönderen hesap dondurulmuş.");

            if (fromAccount.Currency != request.Currency)
                return Result<TransferResponse>.Failure(ErrorCodes.CurrencyMismatch, "Para birimi uyuşmuyor.");

            if (!fromAccount.CanWithdraw(new Money(request.Amount, request.Currency)))
                return Result<TransferResponse>.Failure(ErrorCodes.InsufficientFunds, "Yetersiz bakiye/limit.");

            // IBAN bizim bankamızdaysa alıcıyı da kilitle ve güncelle
            Guid? toAccountId = null;
            Account? internalToAccount = null;
            var internalToAccountLocked = await _accountRepository.GetByIbanForUpdateAsync(request.ToIban, cancellationToken);
            if (internalToAccountLocked is not null)
            {
                // Hesap durumu kontrolleri - alıcı
                if (internalToAccountLocked.Status == AccountStatus.Closed)
                    return Result<TransferResponse>.Failure(ErrorCodes.HesapKapali, "Alıcı hesap kapalı.");
                // Alıcı Frozen olabilir (gelen para serbest)

                if (internalToAccountLocked.Currency != request.Currency)
                    return Result<TransferResponse>.Failure(ErrorCodes.CurrencyMismatch, "Alıcı hesabın para birimi uyuşmuyor.");

                internalToAccount = internalToAccountLocked;
                toAccountId = internalToAccount.Id;
            }

            // Bakiye güncellemeleri
            fromAccount.Withdraw(new Money(request.Amount, request.Currency));
            if (internalToAccount is not null)
            {
                internalToAccount.Deposit(new Money(request.Amount, request.Currency));
            }

            // Transfer kaydı
            var transfer = new Transfer(
                fromAccount.Id,
                toAccountId,
                new Money(request.Amount, request.Currency),
                TransferChannel.EFT,
                request.ToIban
            );
            await _transferRepository.AddAsync(transfer, cancellationToken);

            // Çift kayıt: Debit transaction (gönderen)
            var fromTransaction = new Transaction(
                fromAccount.Id,
                new Money(request.Amount, request.Currency),
                TransactionDirection.Debit,
                request.Description ?? string.Empty
            );
            await _transactionRepository.AddAsync(fromTransaction, cancellationToken);

            // Eğer alıcı bizim bankamızdaysa Credit transaction (alıcı)
            if (internalToAccount is not null)
            {
                var toTransaction = new Transaction(
                    internalToAccount.Id,
                    new Money(request.Amount, request.Currency),
                    TransactionDirection.Credit,
                    request.Description ?? string.Empty
                );
                await _transactionRepository.AddAsync(toTransaction, cancellationToken);
                await _accountRepository.UpdateAsync(internalToAccount, cancellationToken);
            }

            // Gönderen hesabı güncelle
            await _accountRepository.UpdateAsync(fromAccount, cancellationToken);

            var response = new TransferResponse(
                transfer.Id,
                transfer.FromAccountId,
                transfer.ToAccountId ?? Guid.Empty,
                transfer.Amount.Amount,
                transfer.Amount.Currency.ToString(),
                transfer.Channel.ToString(),
                transfer.Status.ToString(),
                transfer.CreatedAt
            );

            return Result<TransferResponse>.Success(response);
        }, ct);

        // Audit log - başarılı transfer
        if (result.IsSuccess && result.Value != null)
        {
            await _auditLogger.LogAsync(
                AuditAction.TransferExternal.ToString(),
                success: true,
                entityType: "Transfer",
                entityId: result.Value.Id.ToString(),
                summary: $"Dış transfer: {request.FromAccountId} -> {request.ToIban}",
                metadata: new { 
                    transferId = result.Value.Id,
                    fromAccountId = request.FromAccountId,
                    toIban = request.ToIban,
                    amount = request.Amount,
                    currency = request.Currency.ToString()
                },
                ct: ct);
        }
        else if (!result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                AuditAction.TransferExternal.ToString(),
                success: false,
                entityType: "Transfer",
                summary: result.ErrorMessage ?? "Transfer başarısız",
                errorCode: result.ErrorCode,
                metadata: new { fromAccountId = request.FromAccountId, toIban = request.ToIban },
                ct: ct);
        }

        return result;
    }

    public async Task<Result<ReverseTransferResponse>> ReverseTransferAsync(ReverseTransferRequest req, CancellationToken ct = default)
    {
        // Sadece Admin iptal edebilir
        if (!_currentUser.IsAdmin)
        {
            await _auditLogger.LogAsync(
                "TransferReverseFailed",
                success: false,
                entityType: "Transfer",
                entityId: req.TransferId.ToString(),
                summary: "Yetkisiz kullanıcı transfer iptal denemesi",
                errorCode: ErrorCodes.Unauthorized,
                ct: ct);
            return Result<ReverseTransferResponse>.Failure(ErrorCodes.Unauthorized, "Sadece admin transfer iptal edebilir.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            // Orijinal transferi FOR UPDATE ile çek
            var original = await _transferRepository.GetByIdForUpdateAsync(req.TransferId, transactionCt);
            if (original == null)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: req.TransferId.ToString(),
                    summary: "Transfer bulunamadı",
                    errorCode: ErrorCodes.NotFound,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.NotFound, "Transfer bulunamadı.");
            }

            // Zaten iptal edilmiş mi?
            if (original.ReversedByTransferId != null)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Transfer zaten iptal edilmiş",
                    errorCode: ErrorCodes.AlreadyReversed,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.AlreadyReversed, "Transfer zaten iptal edilmiş.");
            }

            // Bu bir reversal transfer mi?
            if (original.ReversalOfTransferId != null)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Reversal transfer iptal edilemez",
                    errorCode: ErrorCodes.CannotReverseReversal,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.CannotReverseReversal, "Reversal transfer iptal edilemez.");
            }

            // Zaman kuralı: 30 dakika içinde iptal edilebilir
            var timeSinceCreation = DateTime.UtcNow - original.CreatedAt;
            if (timeSinceCreation > TimeSpan.FromMinutes(30))
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "İptal süresi dolmuş (30 dakika)",
                    errorCode: ErrorCodes.ReversalWindowExpired,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.ReversalWindowExpired, "İptal süresi dolmuş (30 dakika).");
            }

            // External transfer kontrolü
            if (original.Channel == TransferChannel.EFT && original.ToAccountId == null)
            {
                // External transfer ve alıcı bizim bankamızda değil
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Dış transfer iptal edilemez",
                    errorCode: ErrorCodes.ExternalReversalNotSupported,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.ExternalReversalNotSupported, "Dış transfer iptal edilemez.");
            }

            // Reversal için hesapları belirle (yön ters)
            Guid reversalFromAccountId;
            Guid reversalToAccountId;

            if (original.Channel == TransferChannel.Internal)
            {
                // Internal: yönü ters çevir
                reversalFromAccountId = original.ToAccountId!.Value;
                reversalToAccountId = original.FromAccountId;
            }
            else // EFT ama ToAccountId var (bizim bankamızda)
            {
                reversalFromAccountId = original.ToAccountId!.Value;
                reversalToAccountId = original.FromAccountId;
            }

            // Hesapları FOR UPDATE ile kilitle
            var reversalFromAccount = await _accountRepository.GetByIdForUpdateAsync(reversalFromAccountId, transactionCt);
            var reversalToAccount = await _accountRepository.GetByIdForUpdateAsync(reversalToAccountId, transactionCt);

            if (reversalFromAccount == null || reversalToAccount == null)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Hesap bulunamadı",
                    errorCode: ErrorCodes.AccountNotFound,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.AccountNotFound, "Hesap bulunamadı.");
            }

            // Hesap durumu kontrolleri
            if (reversalFromAccount.Status == AccountStatus.Closed || reversalToAccount.Status == AccountStatus.Closed)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Hesap kapalı",
                    errorCode: ErrorCodes.HesapKapali,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.HesapKapali, "Hesap kapalı.");
            }

            if (reversalFromAccount.Status == AccountStatus.Frozen)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Hesap dondurulmuş",
                    errorCode: ErrorCodes.HesapDondurulmus,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.HesapDondurulmus, "Hesap dondurulmuş.");
            }

            // Yetersiz bakiye kontrolü (reversal için fromAccount'ta para olmalı)
            if (!reversalFromAccount.CanWithdraw(original.Amount))
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Yetersiz bakiye (reversal için)",
                    errorCode: ErrorCodes.InsufficientFunds,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.InsufficientFunds, "Yetersiz bakiye (reversal için).");
            }

            // Para birimi kontrolü
            if (reversalFromAccount.Currency != original.Amount.Currency || reversalToAccount.Currency != original.Amount.Currency)
            {
                await _auditLogger.LogAsync(
                    "TransferReverseFailed",
                    success: false,
                    entityType: "Transfer",
                    entityId: original.Id.ToString(),
                    summary: "Para birimi uyuşmuyor",
                    errorCode: ErrorCodes.CurrencyMismatch,
                    ct: transactionCt);
                return Result<ReverseTransferResponse>.Failure(ErrorCodes.CurrencyMismatch, "Para birimi uyuşmuyor.");
            }

            // Reversal transfer oluştur
            var reversal = new Transfer(
                reversalFromAccountId,
                reversalToAccountId,
                original.Amount,
                original.Channel,
                original.ExternalIban,
                original.Id // ReversalOfTransferId
            );
            await _transferRepository.AddAsync(reversal, transactionCt);

            // Bakiyeleri güncelle (reversal)
            reversalFromAccount.Withdraw(original.Amount);
            reversalToAccount.Deposit(original.Amount);

            // 2 transaction oluştur (Debit/Credit) reversal için
            var reversalDescription = $"Reversal: {req.Reason ?? "Transfer iptali"}";
            var fromTransaction = new Transaction(
                reversalFromAccountId,
                original.Amount,
                TransactionDirection.Debit,
                reversalDescription
            );

            var toTransaction = new Transaction(
                reversalToAccountId,
                original.Amount,
                TransactionDirection.Credit,
                reversalDescription
            );

            await _transactionRepository.AddAsync(fromTransaction, transactionCt);
            await _transactionRepository.AddAsync(toTransaction, transactionCt);

            // Orijinal transferi işaretle
            original.MarkReversed(reversal.Id, DateTime.UtcNow);

            // Hesapları güncelle
            await _accountRepository.UpdateAsync(reversalFromAccount, transactionCt);
            await _accountRepository.UpdateAsync(reversalToAccount, transactionCt);

            // Orijinal transferi güncelle
            await _transferRepository.UpdateAsync(original, transactionCt);

            var response = new ReverseTransferResponse(
                original.Id,
                reversal.Id,
                original.ReversedAt!.Value
            );

            return Result<ReverseTransferResponse>.Success(response);
        }, ct);

        // Audit log
        if (result.IsSuccess && result.Value != null)
        {
            await _auditLogger.LogAsync(
                "TransferReversed",
                success: true,
                entityType: "Transfer",
                entityId: req.TransferId.ToString(),
                summary: $"Transfer iptal edildi: {req.Reason ?? "Neden belirtilmedi"}",
                metadata: new
                {
                    originalTransferId = result.Value.OriginalTransferId,
                    reversalTransferId = result.Value.ReversalTransferId,
                    reason = req.Reason
                },
                ct: ct);
        }
        else if (!result.IsSuccess)
        {
            await _auditLogger.LogAsync(
                "TransferReverseFailed",
                success: false,
                entityType: "Transfer",
                entityId: req.TransferId.ToString(),
                summary: result.ErrorMessage ?? "Transfer iptal başarısız",
                errorCode: result.ErrorCode,
                metadata: new { reason = req.Reason },
                ct: ct);
        }

        return result;
    }
}

