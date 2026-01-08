using NovaBank.Application.Common.Results;
using NovaBank.Contracts.CreditCards;

namespace NovaBank.Application.CreditCards;

public interface ICreditCardService
{
    // Müşteri işlemleri
    Task<Result> ApplyForCreditCardAsync(ApplyCreditCardRequest req, CancellationToken ct = default);
    Task<Result<List<CreditCardSummaryResponse>>> GetMyCardsAsync(CancellationToken ct = default);
    Task<Result<List<MyApplicationResponse>>> GetMyApplicationsAsync(CancellationToken ct = default);
    Task<Result> MakeCardPaymentAsync(Guid cardId, decimal amount, Guid fromAccountId, CancellationToken ct = default);
    
    // Admin işlemleri
    Task<Result<List<CreditCardApplicationResponse>>> GetPendingApplicationsAsync(CancellationToken ct = default);
    Task<Result> ApproveApplicationAsync(Guid applicationId, decimal approvedLimit, CancellationToken ct = default);
    Task<Result> RejectApplicationAsync(Guid applicationId, string reason, CancellationToken ct = default);
    Task<Result> ProcessInterestsAsync(CancellationToken ct = default);
}
