using NovaBank.Contracts.Kyc;
using NovaBank.Core.Enums;

namespace NovaBank.Application.Kyc;

public interface IKycService
{
    Task<KycVerificationResponse> SubmitVerificationAsync(CreateKycVerificationRequest request, CancellationToken ct = default);
    Task<List<KycVerificationResponse>> GetMyVerificationsAsync(CancellationToken ct = default);
    Task<List<KycVerificationResponse>> GetPendingVerificationsAsync(CancellationToken ct = default); // Admin/Manager
    
    Task VerifyAsync(Guid id, CancellationToken ct = default);
    Task RejectAsync(Guid id, string reason, CancellationToken ct = default);
}
