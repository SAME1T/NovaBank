using Mapster;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Contracts.Kyc;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Exceptions;

namespace NovaBank.Application.Kyc;

public class KycService : IKycService
{
    private readonly IKycRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public KycService(IKycRepository repository, ICustomerRepository customerRepository, IUnitOfWork unitOfWork, CurrentUser currentUser)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<KycVerificationResponse> SubmitVerificationAsync(CreateKycVerificationRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
             throw new UnauthorizedAccessException("User is not authenticated.");

        var verification = new KycVerification(
            _currentUser.CustomerId.Value,
            request.VerificationType,
            request.DocumentPath,
            request.ExpiryDate
        );

        await _repository.AddAsync(verification, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return verification.Adapt<KycVerificationResponse>();
    }

    public async Task<List<KycVerificationResponse>> GetMyVerificationsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
             throw new UnauthorizedAccessException("User is not authenticated.");

        var list = await _repository.GetByCustomerIdAsync(_currentUser.CustomerId.Value, ct);
        return list.Adapt<List<KycVerificationResponse>>();
    }

    public async Task<List<KycVerificationResponse>> GetPendingVerificationsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin && !_currentUser.IsManager)
             throw new UnauthorizedAccessException("Insufficient permissions.");

        var list = await _repository.GetPendingAsync(ct);
        return list.Adapt<List<KycVerificationResponse>>();
    }

    public async Task VerifyAsync(Guid id, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin && !_currentUser.IsManager)
             throw new UnauthorizedAccessException("Insufficient permissions.");

        var verification = await _repository.GetByIdAsync(id, ct);
        if (verification == null) throw new NotFoundException("Verification record not found.");
        
        if (verification.Status != VerificationStatus.Pending)
            throw new InvalidOperationException("Only pending verifications can be verified.");

        verification.Verify(_currentUser.CustomerId.Value);
        
        // Update Customer KycStatus if needed
        var customer = await _customerRepository.GetByIdAsync(verification.CustomerId, ct);
        if (customer != null)
        {
            // If ID verified, maybe mark KycCompleted?
            if (verification.VerificationType == VerificationType.Identity)
            {
                customer.CompleteKyc();
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(Guid id, string reason, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin && !_currentUser.IsManager)
             throw new UnauthorizedAccessException("Insufficient permissions.");

        var verification = await _repository.GetByIdAsync(id, ct);
        if (verification == null) throw new NotFoundException("Verification record not found.");
        
        if (verification.Status != VerificationStatus.Pending)
            throw new InvalidOperationException("Only pending verifications can be rejected.");

        verification.Reject(_currentUser.CustomerId.Value, reason);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
