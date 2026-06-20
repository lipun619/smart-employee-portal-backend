namespace SmartEmployeePortal.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern: wraps multiple repository operations in a single
/// database transaction. Call SaveChangesAsync() once after all operations
/// to commit everything atomically — either all succeed or all roll back.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IDepartmentRepository Departments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
