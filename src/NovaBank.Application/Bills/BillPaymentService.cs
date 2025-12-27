using Mapster;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Application.Commissions; // For CommissionService
using NovaBank.Contracts.Bills;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Exceptions;
using NovaBank.Core.ValueObjects;

namespace NovaBank.Application.Bills;

public class BillPaymentService : IBillPaymentService
{
    private readonly IBillInstitutionRepository _institutionRepository;
    private readonly IBillPaymentRepository _paymentRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICardRepository _cardRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICommissionService _commissionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public BillPaymentService(
        IBillInstitutionRepository institutionRepository,
        IBillPaymentRepository paymentRepository,
        IAccountRepository accountRepository,
        ICardRepository cardRepository,
        ITransactionRepository transactionRepository,
        ICommissionService commissionService,
        IUnitOfWork unitOfWork,
        CurrentUser currentUser)
    {
        _institutionRepository = institutionRepository;
        _paymentRepository = paymentRepository;
        _accountRepository = accountRepository;
        _cardRepository = cardRepository;
        _transactionRepository = transactionRepository;
        _commissionService = commissionService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<List<BillInstitutionResponse>> GetInstitutionsAsync(CancellationToken ct = default)
    {
        var list = await _institutionRepository.GetActiveAsync(ct);
        return list.Adapt<List<BillInstitutionResponse>>();
    }

    public async Task<BillInquiryResponse> InquireAsync(BillInquiryRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var institution = await _institutionRepository.GetByIdAsync(request.InstitutionId, ct);
        if (institution == null) throw new NotFoundException("Institution not found.");

        // MOCK LOGIC: Generate random bill amount
        // In real world, call external service
        var random = new Random(request.SubscriberNo.GetHashCode());
        decimal amount = random.Next(50, 500) + (decimal)random.NextDouble();
        amount = Math.Round(amount, 2);

        // Calculate Commission
        decimal commission = await _commissionService.CalculateCommissionAsync(CommissionType.BillPayment, Currency.TRY, amount, ct);
        
        return new BillInquiryResponse(
            institution.Id,
            institution.Name,
            request.SubscriberNo,
            "Örnek Abone", // Mock Name
            amount,
            commission,
            DateTime.Today.AddDays(random.Next(1, 15)), // Mock Due Date
            Guid.NewGuid().ToString("N")[..10].ToUpper() // Mock Invoice No
        );
    }

    public async Task<BillPaymentResponse> PayAsync(PayBillRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var institution = await _institutionRepository.GetByIdAsync(request.InstitutionId, ct);
        if (institution == null) throw new NotFoundException("Institution not found.");

        // Calculate Commission
        decimal commission = await _commissionService.CalculateCommissionAsync(CommissionType.BillPayment, Currency.TRY, request.Amount, ct);
        decimal total = request.Amount + commission;
        
        BillPayment payment;

        if (request.CardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(request.CardId.Value, ct);
            if (card == null) throw new NotFoundException("Card not found.");
            
            if (card.CustomerId != _currentUser.CustomerId)
                 throw new UnauthorizedAccessException("Cannot access this card.");

            if (card.CardType != CardType.Credit)
                 throw new InvalidOperationException("Fatura ödemesi sadece kredi kartı ile yapılabilir.");

            // Harcama ekle
            card.AddSpending(total);

            payment = new BillPayment(
                null,
                card.Id,
                institution.Id,
                request.SubscriberNo,
                request.Amount,
                commission,
                null,
                request.InvoiceNo
            );
        }
        else if (request.AccountId.HasValue)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.Value, ct);
            if (account == null) throw new NotFoundException("Account not found.");
            
            if (!_currentUser.CanAccessAccount(account.CustomerId))
                 throw new UnauthorizedAccessException("Cannot access this account.");

            var totalMoney = new Money(total, account.Currency);
            var billMoney = new Money(request.Amount, account.Currency);
            var commMoney = new Money(commission, account.Currency);

            // Withdraw
            if (!account.CanWithdraw(totalMoney))
                throw new InvalidOperationException("Insufficient funds.");
                
            account.Withdraw(totalMoney);

            payment = new BillPayment(
                account.Id,
                null,
                institution.Id,
                request.SubscriberNo,
                request.Amount,
                commission,
                null,
                request.InvoiceNo
            );

            // Create Transactions for Account
            var txBill = new Transaction(
                account.Id, 
                billMoney, 
                TransactionDirection.Debit, 
                $"Fatura Ödeme: {institution.Name} - {request.SubscriberNo}");
            await _transactionRepository.AddAsync(txBill, ct);

            if (commission > 0)
            {
                 var txComm = new Transaction(
                    account.Id, 
                    commMoney, 
                    TransactionDirection.Debit, 
                    $"Komisyon: Fatura Ödeme ({institution.Name})");
                 await _transactionRepository.AddAsync(txComm, ct);
            }
        }
        else
        {
            throw new ArgumentException("Ödeme için bir hesap veya kart seçmelisiniz.");
        }

        await _paymentRepository.AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = payment.Adapt<BillPaymentResponse>();
        // Map Name manually as it's not in BillPayment entity
        return response with { InstitutionName = institution.Name, TotalAmount = total };
    }

    public async Task<List<BillPaymentResponse>> GetHistoryAsync(Guid accountId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();
        
        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account == null) throw new NotFoundException("Account not found.");
        
        if (!_currentUser.CanAccessAccount(account.CustomerId))
             throw new UnauthorizedAccessException("Cannot access this account.");

        var history = await _paymentRepository.GetByAccountIdAsync(accountId, ct);
        
        // Need to join with Institution Name efficiently?
        // Mapster adaption might miss InstitutionName.
        // For now, load institutions or efficient query.
        // Or assume UI shows details.
        
        // Quick fix: Adapt
        var responses = new List<BillPaymentResponse>();
        var institutions = await _institutionRepository.GetAllAsync(ct);
        var dict = institutions.ToDictionary(x => x.Id, x => x.Name);

        foreach (var p in history)
        {
            var res = p.Adapt<BillPaymentResponse>();
            if (dict.TryGetValue(p.InstitutionId, out var name))
            {
                res = res with { InstitutionName = name };
            }
            responses.Add(res);
        }
        return responses;
    }

    public async Task<List<BillPaymentResponse>> GetCustomerHistoryAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();
        if (!_currentUser.CustomerId.HasValue) throw new UnauthorizedAccessException("Customer profile required.");

        var history = await _paymentRepository.GetByCustomerIdAsync(_currentUser.CustomerId.Value, ct);
        
        var responses = new List<BillPaymentResponse>();
        var institutions = await _institutionRepository.GetAllAsync(ct);
        var dict = institutions.ToDictionary(x => x.Id, x => x.Name);

        foreach (var p in history)
        {
            var res = p.Adapt<BillPaymentResponse>();
            if (dict.TryGetValue(p.InstitutionId, out var name))
            {
                res = res with { InstitutionName = name };
            }
            responses.Add(res);
        }
        return responses;
    }

    public async Task<BillInstitutionResponse> CreateInstitutionAsync(CreateBillInstitutionRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin) throw new UnauthorizedAccessException("Admin privileges required.");

        var institution = new BillInstitution(request.Code, request.Name, request.Category, request.LogoUrl);
        await _institutionRepository.AddAsync(institution, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Adapt<BillInstitutionResponse>();
    }

    public async Task DeleteInstitutionAsync(Guid id, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin) throw new UnauthorizedAccessException("Admin privileges required.");

        await _institutionRepository.DeleteAsync(id, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
