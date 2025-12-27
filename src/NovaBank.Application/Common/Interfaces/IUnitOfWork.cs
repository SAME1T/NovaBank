namespace NovaBank.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Executes an action within a database transaction.
    /// If the action completes successfully, the transaction is committed.
    /// If an exception occurs, the transaction is rolled back.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);

    /// <summary>
    /// Executes an action within a database transaction and returns a result.
    /// If the action completes successfully, the transaction is committed.
    /// If an exception occurs, the transaction is rolled back.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default);
}


