using Microsoft.EntityFrameworkCore.Storage;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Infrastructure.Persistence;

namespace NovaBank.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Entity Framework implementation of Unit of Work pattern.
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly BankDbContext _context;

    public EfUnitOfWork(BankDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        await ExecuteInTransactionAsync(async (cancellationToken) =>
        {
            await action(cancellationToken);
            return Task.CompletedTask;
        }, ct);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            // Already in a transaction, just execute the action
            return await action(ct);
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            var result = await action(ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
            }
            throw;
        }
    }
}

