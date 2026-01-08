using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Common.Results;
using NovaBank.Application.Commissions;
using NovaBank.Contracts.CurrencyExchange;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.CurrencyExchange;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrencyPositionRepository _positionRepository;
    private readonly ICurrencyTransactionRepository _currencyTransactionRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ICommissionService _commissionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;
    private readonly CurrentUser _currentUser;

    private const decimal MIN_BUY_AMOUNT = 10m; // Minimum 10 döviz birimi
    private const decimal MIN_SELL_AMOUNT = 1m; // Minimum 1 döviz birimi

    public CurrencyExchangeService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ICurrencyPositionRepository positionRepository,
        ICurrencyTransactionRepository currencyTransactionRepository,
        IExchangeRateRepository exchangeRateRepository,
        ICommissionService commissionService,
        IUnitOfWork unitOfWork,
        IAuditLogger auditLogger,
        CurrentUser currentUser)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _currencyTransactionRepository = currencyTransactionRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _commissionService = commissionService;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _currentUser = currentUser;
    }

    public async Task<Result<CurrencyExchangeResponse>> BuyCurrencyAsync(BuyCurrencyRequest request, CancellationToken ct = default)
    {
        // Validasyonlar
        if (request.Currency == Currency.TRY)
        {
            return Result<CurrencyExchangeResponse>.Failure("FX_INVALID_CURRENCY", "TL alınamaz.");
        }

        if (request.Amount < MIN_BUY_AMOUNT)
        {
            return Result<CurrencyExchangeResponse>.Failure("FX_MIN_AMOUNT", $"Minimum alım miktarı {MIN_BUY_AMOUNT} {request.Currency}.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            // Hesapları getir ve kilitle
            var tryAccount = await _accountRepository.GetByIdForUpdateAsync(request.FromTryAccountId, cancellationToken);
            var foreignAccount = await _accountRepository.GetByIdForUpdateAsync(request.ToForeignAccountId, cancellationToken);

            if (tryAccount == null)
                return Result<CurrencyExchangeResponse>.Failure(ErrorCodes.AccountNotFound, "TL hesabı bulunamadı.");
            if (foreignAccount == null)
                return Result<CurrencyExchangeResponse>.Failure(ErrorCodes.AccountNotFound, "Döviz hesabı bulunamadı.");

            // Hesap kontrolleri
            if (tryAccount.Currency != Currency.TRY)
                return Result<CurrencyExchangeResponse>.Failure("FX_TRY_ACCOUNT_REQUIRED", "Kaynak hesap TL hesabı olmalı.");
            if (foreignAccount.Currency != request.Currency)
                return Result<CurrencyExchangeResponse>.Failure("FX_CURRENCY_MISMATCH", $"Hedef hesap {request.Currency} hesabı olmalı.");
            if (tryAccount.CustomerId != _currentUser.CustomerId || foreignAccount.CustomerId != _currentUser.CustomerId)
                return Result<CurrencyExchangeResponse>.Failure(ErrorCodes.Unauthorized, "Sadece kendi hesaplarınız arasında işlem yapabilirsiniz.");
            if (tryAccount.Status != AccountStatus.Active)
                return Result<CurrencyExchangeResponse>.Failure("ACCOUNT_NOT_ACTIVE", "TL hesabı aktif değil.");
            if (foreignAccount.Status != AccountStatus.Active)
                return Result<CurrencyExchangeResponse>.Failure("ACCOUNT_NOT_ACTIVE", "Döviz hesabı aktif değil.");

            // Kur getir
            var rate = await _exchangeRateRepository.GetLatestAsync(Currency.TRY, request.Currency, cancellationToken);
            if (rate == null)
            {
                return Result<CurrencyExchangeResponse>.Failure("FX_RATE_NOT_FOUND", $"{request.Currency} kuru bulunamadı.");
            }

            // Kur güncelliği kontrolü (1 günden eski olmamalı)
            if (rate.EffectiveDate < DateTime.UtcNow.Date.AddDays(-1))
            {
                return Result<CurrencyExchangeResponse>.Failure("FX_RATE_EXPIRED", "Döviz kuru güncel değil. Lütfen kurları yenileyin.");
            }

            // Hesaplama: Müşteri döviz alıyor = Banka satış kuru kullanılır
            var tryAmount = rate.CalculateBuy(request.Amount); // amount * SellRate
            
            // Komisyon hesapla
            var commission = await _commissionService.CalculateCommissionAsync(CommissionType.CurrencyBuy, Currency.TRY, tryAmount, cancellationToken);
            var totalTry = tryAmount + commission;

            // Bakiye kontrolü
            var availableBalance = tryAccount.Balance.Amount + tryAccount.OverdraftLimit;
            if (totalTry > availableBalance)
            {
                return Result<CurrencyExchangeResponse>.Failure("INSUFFICIENT_TRY_BALANCE", 
                    $"Yetersiz TL bakiyesi. Gerekli: {totalTry:N2} TL, Kullanılabilir: {availableBalance:N2} TL");
            }

            // Mevcut pozisyonu getir veya oluştur
            var position = await _positionRepository.GetByCustomerAndCurrencyAsync(_currentUser.CustomerId!.Value, request.Currency, cancellationToken);
            var positionBefore = position?.TotalAmount ?? 0;
            var avgCostBefore = position?.AverageCostRate ?? 0;

            if (position == null)
            {
                position = new CurrencyPosition(_currentUser.CustomerId!.Value, request.Currency);
                await _positionRepository.AddAsync(position, cancellationToken);
            }

            // TL hesaptan çek
            tryAccount.Withdraw(new Money(totalTry, Currency.TRY));

            // Döviz hesaba yatır
            foreignAccount.Deposit(new Money(request.Amount, request.Currency));

            // Pozisyonu güncelle
            position.AddPosition(request.Amount, totalTry);

            // Transaction kayıtları
            var tryTransaction = new Transaction(
                tryAccount.Id,
                new Money(totalTry, Currency.TRY),
                TransactionDirection.Debit,
                $"Döviz alımı: {request.Amount} {request.Currency} @ {rate.SellRate:N4}"
            );
            await _transactionRepository.AddAsync(tryTransaction, cancellationToken);

            var foreignTransaction = new Transaction(
                foreignAccount.Id,
                new Money(request.Amount, request.Currency),
                TransactionDirection.Credit,
                $"Döviz alımı: {request.Amount} {request.Currency}"
            );
            await _transactionRepository.AddAsync(foreignTransaction, cancellationToken);

            // FX Transaction kaydı
            var fxTransaction = CurrencyTransaction.CreateBuy(
                _currentUser.CustomerId!.Value,
                request.Currency,
                request.Amount,
                rate.SellRate,
                rate.EffectiveDate,
                tryAmount,
                commission,
                request.FromTryAccountId,
                request.ToForeignAccountId,
                positionBefore,
                position.TotalAmount,
                avgCostBefore,
                position.AverageCostRate,
                request.Description
            );
            await _currencyTransactionRepository.AddAsync(fxTransaction, cancellationToken);

            // Audit log
            await _auditLogger.LogAsync(
                "CurrencyBuy",
                success: true,
                entityType: "CurrencyTransaction",
                entityId: fxTransaction.Id.ToString(),
                summary: $"{request.Amount} {request.Currency} alındı @ {rate.SellRate:N4}",
                metadata: new
                {
                    currency = request.Currency.ToString(),
                    amount = request.Amount,
                    rate = rate.SellRate,
                    tryAmount,
                    commission,
                    totalTry
                },
                ct: cancellationToken
            );

            return Result<CurrencyExchangeResponse>.Success(new CurrencyExchangeResponse(
                fxTransaction.Id,
                fxTransaction.ReferenceCode,
                request.Currency.ToString(),
                request.Amount,
                rate.SellRate,
                tryAmount,
                commission,
                totalTry,
                null,
                null,
                new PositionSnapshot(position.TotalAmount, position.AverageCostRate, position.TotalCostTry)
            ));
        }, ct);

        return result;
    }

    public async Task<Result<CurrencyExchangeResponse>> SellCurrencyAsync(SellCurrencyRequest request, CancellationToken ct = default)
    {
        // Validasyonlar
        if (request.Currency == Currency.TRY)
        {
            return Result<CurrencyExchangeResponse>.Failure("FX_INVALID_CURRENCY", "TL satılamaz.");
        }

        if (request.Amount < MIN_SELL_AMOUNT)
        {
            return Result<CurrencyExchangeResponse>.Failure("FX_MIN_AMOUNT", $"Minimum satım miktarı {MIN_SELL_AMOUNT} {request.Currency}.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            // Hesapları getir ve kilitle
            var foreignAccount = await _accountRepository.GetByIdForUpdateAsync(request.FromForeignAccountId, cancellationToken);
            var tryAccount = await _accountRepository.GetByIdForUpdateAsync(request.ToTryAccountId, cancellationToken);

            if (foreignAccount == null)
                return Result<CurrencyExchangeResponse>.Failure(ErrorCodes.AccountNotFound, "Döviz hesabı bulunamadı.");
            if (tryAccount == null)
                return Result<CurrencyExchangeResponse>.Failure(ErrorCodes.AccountNotFound, "TL hesabı bulunamadı.");

            // Hesap kontrolleri
            if (foreignAccount.Currency != request.Currency)
                return Result<CurrencyExchangeResponse>.Failure("FX_CURRENCY_MISMATCH", $"Kaynak hesap {request.Currency} hesabı olmalı.");
            if (tryAccount.Currency != Currency.TRY)
                return Result<CurrencyExchangeResponse>.Failure("FX_TRY_ACCOUNT_REQUIRED", "Hedef hesap TL hesabı olmalı.");
            if (foreignAccount.CustomerId != _currentUser.CustomerId || tryAccount.CustomerId != _currentUser.CustomerId)
                return Result<CurrencyExchangeResponse>.Failure(ErrorCodes.Unauthorized, "Sadece kendi hesaplarınız arasında işlem yapabilirsiniz.");
            if (foreignAccount.Status != AccountStatus.Active)
                return Result<CurrencyExchangeResponse>.Failure("ACCOUNT_NOT_ACTIVE", "Döviz hesabı aktif değil.");
            if (tryAccount.Status != AccountStatus.Active)
                return Result<CurrencyExchangeResponse>.Failure("ACCOUNT_NOT_ACTIVE", "TL hesabı aktif değil.");

            // Bakiye kontrolü
            if (foreignAccount.Balance.Amount < request.Amount)
            {
                return Result<CurrencyExchangeResponse>.Failure("INSUFFICIENT_FOREIGN_BALANCE", 
                    $"Yetersiz {request.Currency} bakiyesi. Mevcut: {foreignAccount.Balance.Amount:N2} {request.Currency}");
            }

            // Pozisyon kontrolü
            var position = await _positionRepository.GetByCustomerAndCurrencyAsync(_currentUser.CustomerId!.Value, request.Currency, cancellationToken);
            if (position == null || position.TotalAmount <= 0)
            {
                return Result<CurrencyExchangeResponse>.Failure("FX_NO_POSITION", $"{request.Currency} pozisyonunuz bulunmuyor.");
            }

            if (request.Amount > position.TotalAmount)
            {
                return Result<CurrencyExchangeResponse>.Failure("FX_POSITION_INSUFFICIENT", 
                    $"Pozisyon yetersiz. Mevcut pozisyon: {position.TotalAmount:N2} {request.Currency}");
            }

            // Kur getir
            var rate = await _exchangeRateRepository.GetLatestAsync(Currency.TRY, request.Currency, cancellationToken);
            if (rate == null)
            {
                return Result<CurrencyExchangeResponse>.Failure("FX_RATE_NOT_FOUND", $"{request.Currency} kuru bulunamadı.");
            }

            // Hesaplama: Müşteri döviz satıyor = Banka alış kuru kullanılır
            var tryAmount = rate.CalculateSell(request.Amount); // amount * BuyRate
            
            // Komisyon hesapla
            var commission = await _commissionService.CalculateCommissionAsync(CommissionType.CurrencySell, Currency.TRY, tryAmount, cancellationToken);
            var netTry = tryAmount - commission;

            // Kâr/Zarar hesapla
            var positionBefore = position.TotalAmount;
            var avgCostBefore = position.AverageCostRate;
            var soldCost = position.RemovePosition(request.Amount); // Satılan miktarın maliyeti
            var realizedPnl = netTry - soldCost;
            var realizedPnlPercent = soldCost > 0 ? (realizedPnl / soldCost) * 100 : 0;

            // Döviz hesaptan çek
            foreignAccount.Withdraw(new Money(request.Amount, request.Currency));

            // TL hesaba yatır
            tryAccount.Deposit(new Money(netTry, Currency.TRY));

            // Transaction kayıtları
            var foreignTransaction = new Transaction(
                foreignAccount.Id,
                new Money(request.Amount, request.Currency),
                TransactionDirection.Debit,
                $"Döviz satışı: {request.Amount} {request.Currency} @ {rate.BuyRate:N4}"
            );
            await _transactionRepository.AddAsync(foreignTransaction, cancellationToken);

            var tryTransaction = new Transaction(
                tryAccount.Id,
                new Money(netTry, Currency.TRY),
                TransactionDirection.Credit,
                $"Döviz satışı: {request.Amount} {request.Currency}, K/Z: {realizedPnl:+#,##0.00;-#,##0.00;0} TL"
            );
            await _transactionRepository.AddAsync(tryTransaction, cancellationToken);

            // FX Transaction kaydı
            var fxTransaction = CurrencyTransaction.CreateSell(
                _currentUser.CustomerId!.Value,
                request.Currency,
                request.Amount,
                rate.BuyRate,
                rate.EffectiveDate,
                tryAmount,
                commission,
                request.FromForeignAccountId,
                request.ToTryAccountId,
                positionBefore,
                position.TotalAmount,
                avgCostBefore,
                position.AverageCostRate,
                Math.Round(realizedPnl, 2),
                Math.Round(realizedPnlPercent, 2),
                request.Description
            );
            await _currencyTransactionRepository.AddAsync(fxTransaction, cancellationToken);

            // Audit log
            await _auditLogger.LogAsync(
                "CurrencySell",
                success: true,
                entityType: "CurrencyTransaction",
                entityId: fxTransaction.Id.ToString(),
                summary: $"{request.Amount} {request.Currency} satıldı @ {rate.BuyRate:N4}, K/Z: {realizedPnl:+#,##0.00;-#,##0.00;0} TL",
                metadata: new
                {
                    currency = request.Currency.ToString(),
                    amount = request.Amount,
                    rate = rate.BuyRate,
                    tryAmount,
                    commission,
                    netTry,
                    realizedPnl,
                    realizedPnlPercent
                },
                ct: cancellationToken
            );

            return Result<CurrencyExchangeResponse>.Success(new CurrencyExchangeResponse(
                fxTransaction.Id,
                fxTransaction.ReferenceCode,
                request.Currency.ToString(),
                request.Amount,
                rate.BuyRate,
                tryAmount,
                commission,
                netTry,
                Math.Round(realizedPnl, 2),
                Math.Round(realizedPnlPercent, 2),
                new PositionSnapshot(position.TotalAmount, position.AverageCostRate, position.TotalCostTry)
            ));
        }, ct);

        return result;
    }

    public async Task<Result<CurrencyPositionsResponse>> GetPositionsAsync(CancellationToken ct = default)
    {
        if (_currentUser.CustomerId == null)
            return Result<CurrencyPositionsResponse>.Failure(ErrorCodes.Unauthorized, "Oturum açmanız gerekiyor.");

        var positions = await _positionRepository.GetByCustomerIdAsync(_currentUser.CustomerId.Value, ct);
        var rates = await _exchangeRateRepository.GetAllLatestAsync(ct);

        var positionResponses = new List<CurrencyPositionResponse>();
        decimal totalCost = 0;
        decimal totalCurrentValue = 0;

        foreach (var pos in positions)
        {
            if (pos.TotalAmount <= 0) continue;

            var rate = rates.FirstOrDefault(r => r.TargetCurrency == pos.Currency);
            var currentRate = rate?.BuyRate ?? 0;
            var currentValue = pos.CalculateCurrentValue(currentRate);
            var (pnlTry, pnlPercent) = pos.CalculateUnrealizedPnl(currentRate);

            positionResponses.Add(new CurrencyPositionResponse(
                pos.Currency.ToString(),
                pos.TotalAmount,
                pos.AverageCostRate,
                pos.TotalCostTry,
                currentRate,
                currentValue,
                pnlTry,
                pnlPercent
            ));

            totalCost += pos.TotalCostTry;
            totalCurrentValue += currentValue;
        }

        var totalPnl = totalCurrentValue - totalCost;
        var totalPnlPercent = totalCost > 0 ? (totalPnl / totalCost) * 100 : 0;

        return Result<CurrencyPositionsResponse>.Success(new CurrencyPositionsResponse(
            positionResponses,
            Math.Round(totalCost, 2),
            Math.Round(totalCurrentValue, 2),
            Math.Round(totalPnl, 2),
            Math.Round(totalPnlPercent, 2)
        ));
    }

    public async Task<Result<(decimal BuyRate, decimal SellRate, DateTime RateDate)>> GetCurrentRateAsync(Currency currency, CancellationToken ct = default)
    {
        if (currency == Currency.TRY)
            return Result<(decimal, decimal, DateTime)>.Failure("FX_INVALID_CURRENCY", "TL için kur sorgulanamaz.");

        var rate = await _exchangeRateRepository.GetLatestAsync(Currency.TRY, currency, ct);
        if (rate == null)
            return Result<(decimal, decimal, DateTime)>.Failure("FX_RATE_NOT_FOUND", $"{currency} kuru bulunamadı.");

        return Result<(decimal BuyRate, decimal SellRate, DateTime RateDate)>.Success((rate.BuyRate, rate.SellRate, rate.EffectiveDate));
    }

    public async Task<Result<int>> SaveRatesAsync(SaveExchangeRatesRequest request, CancellationToken ct = default)
    {
        if (request.Rates == null || request.Rates.Count == 0)
            return Result<int>.Failure("FX_NO_RATES", "Kaydedilecek kur bulunamadı.");

        int savedCount = 0;
        
        foreach (var rateItem in request.Rates)
        {
            // Currency kod'u enum'a çevir
            if (!Enum.TryParse<Currency>(rateItem.CurrencyCode, true, out var currency))
                continue; // Bilinmeyen para birimi, atla
                
            if (currency == Currency.TRY)
                continue; // TL kuru kaydetme
                
            if (rateItem.BuyRate <= 0 || rateItem.SellRate <= 0)
                continue; // Geçersiz kur
                
            try
            {
                // Tarih değerini UTC olarak ayarla
                var rateDateUtc = DateTime.SpecifyKind(request.RateDate.Date, DateTimeKind.Utc);
                
                var exchangeRate = new ExchangeRate(
                    Currency.TRY,
                    currency,
                    rateItem.BuyRate,
                    rateItem.SellRate,
                    rateDateUtc,
                    "TCMB"
                );
                
                await _exchangeRateRepository.AddOrUpdateAsync(exchangeRate, ct);
                savedCount++;
            }
            catch
            {
                // Hata olursa atla, diğer kurları kaydetmeye devam et
            }
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result<int>.Success(savedCount);
    }
}
