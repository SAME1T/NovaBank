using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Application.Transfers;
using NovaBank.Contracts.Transactions;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Transactions;

public class TransactionsService : ITransactionsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransfersService _transfersService;

    public TransactionsService(
        IAccountRepository accountRepository,
        ITransfersService transfersService)
    {
        _accountRepository = accountRepository;
        _transfersService = transfersService;
    }

    public async Task<Result<TransactionResponse>> DepositAsync(DepositRequest request, CancellationToken ct = default)
    {
        // Sistem kasa hesabını bul
        var cashAccount = await _accountRepository.GetByIbanAsync(SystemAccounts.CashTryIban, ct);
        if (cashAccount is null)
            return Result<TransactionResponse>.Failure(ErrorCodes.AccountNotFound, "Sistem kasa hesabı bulunamadı. Lütfen sistem yöneticisine başvurun.");

        // Müşteri hesabını kontrol et
        var customerAccount = await _accountRepository.GetByIdAsync(request.AccountId, ct);
        if (customerAccount is null)
            return Result<TransactionResponse>.Failure(ErrorCodes.AccountNotFound, "Hesap bulunamadı.");

        if (customerAccount.Currency != request.Currency)
            return Result<TransactionResponse>.Failure(ErrorCodes.CurrencyMismatch, "Para birimi uyuşmuyor.");

        if (request.Amount <= 0)
            return Result<TransactionResponse>.Failure(ErrorCodes.InvalidAmount, "Tutar pozitif olmalı.");

        // Deposit = SYSTEM_CASH_ACCOUNT -> CustomerAccount (transfer)
        var transferRequest = new TransferInternalRequest(
            cashAccount.Id,
            customerAccount.Id,
            request.Amount,
            request.Currency,
            request.Description ?? "Para yatırma"
        );

        var transferResult = await _transfersService.TransferInternalAsync(transferRequest, ct);
        if (!transferResult.IsSuccess)
            return Result<TransactionResponse>.Failure(transferResult.ErrorCode!, transferResult.ErrorMessage!);

        // Transfer başarılı, Credit transaction'ı döndür (müşteri hesabına gelen)
        // TransferResponse'dan transaction bilgisi çıkaramayız, bu yüzden basit bir response döndürüyoruz
        // Gerçek uygulamada Transfer entity'sinden transaction'ları çekebiliriz
        return Result<TransactionResponse>.Success(new TransactionResponse(
            transferResult.Value!.Id, // Transfer ID'yi kullanıyoruz
            customerAccount.Id,
            request.Amount,
            request.Currency.ToString(),
            "Credit",
            request.Description ?? "Para yatırma",
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow
        ));
    }

    public async Task<Result<TransactionResponse>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default)
    {
        // Sistem kasa hesabını bul
        var cashAccount = await _accountRepository.GetByIbanAsync(SystemAccounts.CashTryIban, ct);
        if (cashAccount is null)
            return Result<TransactionResponse>.Failure(ErrorCodes.AccountNotFound, "Sistem kasa hesabı bulunamadı. Lütfen sistem yöneticisine başvurun.");

        // Müşteri hesabını kontrol et
        var customerAccount = await _accountRepository.GetByIdAsync(request.AccountId, ct);
        if (customerAccount is null)
            return Result<TransactionResponse>.Failure(ErrorCodes.AccountNotFound, "Hesap bulunamadı.");

        if (customerAccount.Currency != request.Currency)
            return Result<TransactionResponse>.Failure(ErrorCodes.CurrencyMismatch, "Para birimi uyuşmuyor.");

        if (request.Amount <= 0)
            return Result<TransactionResponse>.Failure(ErrorCodes.InvalidAmount, "Tutar pozitif olmalı.");

        // Withdraw = CustomerAccount -> SYSTEM_CASH_ACCOUNT (transfer)
        var transferRequest = new TransferInternalRequest(
            customerAccount.Id,
            cashAccount.Id,
            request.Amount,
            request.Currency,
            request.Description ?? "Para çekme"
        );

        var transferResult = await _transfersService.TransferInternalAsync(transferRequest, ct);
        if (!transferResult.IsSuccess)
            return Result<TransactionResponse>.Failure(transferResult.ErrorCode!, transferResult.ErrorMessage!);

        // Transfer başarılı, Debit transaction'ı döndür (müşteri hesabından çıkan)
        return Result<TransactionResponse>.Success(new TransactionResponse(
            transferResult.Value!.Id, // Transfer ID'yi kullanıyoruz
            customerAccount.Id,
            request.Amount,
            request.Currency.ToString(),
            "Debit",
            request.Description ?? "Para çekme",
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow
        ));
    }
}

