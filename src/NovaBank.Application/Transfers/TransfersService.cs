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

    public TransfersService(
        IAccountRepository accountRepository,
        ITransferRepository transferRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TransferResponse>> TransferInternalAsync(TransferInternalRequest request, CancellationToken ct = default)
    {
        if (request.FromAccountId == request.ToAccountId)
            return Result<TransferResponse>.Failure(ErrorCodes.SameAccountTransfer, "Aynı hesaba transfer olmaz.");

        if (request.Amount <= 0)
            return Result<TransferResponse>.Failure(ErrorCodes.InvalidAmount, "Tutar pozitif olmalı.");

        return await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            // FOR UPDATE ile hesapları kilitle
            var fromAccount = await _accountRepository.GetByIdForUpdateAsync(request.FromAccountId, cancellationToken);
            var toAccount = await _accountRepository.GetByIdForUpdateAsync(request.ToAccountId, cancellationToken);

            if (fromAccount is null || toAccount is null)
                return Result<TransferResponse>.Failure(ErrorCodes.AccountNotFound, "Gönderen veya alıcı hesap bulunamadı.");

            if (fromAccount.Currency != toAccount.Currency || fromAccount.Currency != request.Currency)
                return Result<TransferResponse>.Failure(ErrorCodes.CurrencyMismatch, "Para birimi uyuşmuyor.");

            if (!fromAccount.CanWithdraw(new Money(request.Amount, request.Currency)))
                return Result<TransferResponse>.Failure(ErrorCodes.InsufficientFunds, "Yetersiz bakiye/limit.");

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
    }

    public async Task<Result<TransferResponse>> TransferExternalAsync(TransferExternalRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return Result<TransferResponse>.Failure(ErrorCodes.InvalidAmount, "Tutar pozitif olmalı.");

        return await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            // FOR UPDATE ile gönderen hesabı kilitle
            var fromAccount = await _accountRepository.GetByIdForUpdateAsync(request.FromAccountId, cancellationToken);
            if (fromAccount is null)
                return Result<TransferResponse>.Failure(ErrorCodes.AccountNotFound, "Gönderen hesap bulunamadı.");

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
    }
}

