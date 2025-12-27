using Mapster;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Contracts.Limits;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Exceptions;

namespace NovaBank.Application.Limits;

public class LimitService : ILimitService
{
    private readonly ITransactionLimitRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public LimitService(ITransactionLimitRepository repository, IUnitOfWork unitOfWork, CurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<TransactionLimitResponse> CreateLimitAsync(CreateLimitRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Only admins can manage limits.");

        var limit = new TransactionLimit(
            request.LimitType,
            request.Scope,
            request.Currency,
            request.MaxAmount,
            request.MinAmount,
            request.ScopeId,
            request.ScopeRole,
            request.RequiresApprovalAbove
        );

        await _repository.AddAsync(limit, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return limit.Adapt<TransactionLimitResponse>();
    }

    public async Task<List<TransactionLimitResponse>> GetActiveLimitsAsync(CancellationToken ct = default)
    {
        var limits = await _repository.GetActiveLimitsAsync(ct);
        return limits.Adapt<List<TransactionLimitResponse>>();
    }

    public async Task UpdateLimitAsync(Guid id, UpdateLimitRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Only admins can manage limits.");

        var limit = await _repository.GetByIdAsync(id, ct);
        if (limit == null) throw new NotFoundException("Limit not found.");

        limit.Update(request.MaxAmount, request.MinAmount, request.RequiresApprovalAbove);
        
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) limit.Activate();
            else limit.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateLimitAsync(Guid id, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Only admins can manage limits.");
            
        var limit = await _repository.GetByIdAsync(id, ct);
        if (limit != null)
        {
            limit.Deactivate();
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<(bool IsAllowed, string? Error)> CheckLimitAsync(LimitType type, LimitScope scope, Currency currency, decimal amount, Guid? scopeId = null, UserRole? role = null, CancellationToken ct = default)
    {
        var limit = await _repository.GetApplicableLimitAsync(type, scope, currency, scopeId, role, ct);
        if (limit == null) return (true, null); // No limit defined = Allowed

        if (!limit.IsWithinLimit(amount))
            return (false, $"Amount {amount} {currency} is outside the allowed range ({limit.MinAmount}-{limit.MaxAmount}).");

        return (true, null);
    }

    public async Task<bool> RequiresApprovalAsync(LimitType type, LimitScope scope, Currency currency, decimal amount, Guid? scopeId = null, UserRole? role = null, CancellationToken ct = default)
    {
        var limit = await _repository.GetApplicableLimitAsync(type, scope, currency, scopeId, role, ct);
        if (limit == null) return false;

        return limit.RequiresApproval(amount);
    }
}
