using Mapster;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Contracts.Commissions;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Exceptions;

namespace NovaBank.Application.Commissions;

public class CommissionService : ICommissionService
{
    private readonly ICommissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public CommissionService(ICommissionRepository repository, IUnitOfWork unitOfWork, CurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CommissionResponse> CreateCommissionAsync(CreateCommissionRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
             throw new UnauthorizedAccessException("Only admins can manage commissions.");

        var commission = new Commission(
            request.CommissionType,
            request.Name,
            request.Currency,
            request.FixedAmount,
            request.PercentageRate,
            request.MinAmount,
            request.MaxAmount,
            request.Description,
            request.ValidFrom,
            request.ValidUntil
        );

        await _repository.AddAsync(commission, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return commission.Adapt<CommissionResponse>();
    }

    public async Task<List<CommissionResponse>> GetActiveCommissionsAsync(CommissionType type, CancellationToken ct = default)
    {
        var commissions = await _repository.GetActiveCommissionsAsync(type, ct);
        return commissions.Adapt<List<CommissionResponse>>();
    }

    public async Task UpdateCommissionAsync(Guid id, UpdateCommissionRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
             throw new UnauthorizedAccessException("Only admins can manage commissions.");

        var commission = await _repository.GetByIdAsync(id, ct);
        if (commission == null) throw new NotFoundException("Commission not found.");

        commission.Update(
            request.Name,
            request.FixedAmount,
            request.PercentageRate,
            request.MinAmount,
            request.MaxAmount,
            request.Description,
            request.ValidUntil
        );
        
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) commission.Activate();
            else commission.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<decimal> CalculateCommissionAsync(CommissionType type, Currency currency, decimal amount, CancellationToken ct = default)
    {
        var commissions = await _repository.GetActiveCommissionsAsync(type, ct);
        
        // Filter by currency
        var applicable = commissions.Where(c => c.Currency == currency).ToList();
        
        // If multiple commissions apply, we usually sum them or take the max?
        // Basic requirement: Sum of all applicable commissions
        decimal total = 0;
        foreach (var comm in applicable)
        {
            total += comm.Calculate(amount);
        }
        
        return total;
    }
}
