using NovaBank.Contracts.Bills;

namespace NovaBank.Application.Bills;

public interface IBillPaymentService
{
    Task<List<BillInstitutionResponse>> GetInstitutionsAsync(CancellationToken ct = default);
    Task<BillInquiryResponse> InquireAsync(BillInquiryRequest request, CancellationToken ct = default);
    Task<BillPaymentResponse> PayAsync(PayBillRequest request, CancellationToken ct = default);
    Task<List<BillPaymentResponse>> GetHistoryAsync(Guid accountId, CancellationToken ct = default);
    Task<List<BillPaymentResponse>> GetCustomerHistoryAsync(CancellationToken ct = default);

    // Admin
    Task<BillInstitutionResponse> CreateInstitutionAsync(CreateBillInstitutionRequest request, CancellationToken ct = default);
    Task DeleteInstitutionAsync(Guid id, CancellationToken ct = default);
}
