using SmartEmployeePortal.Domain.Common;
using SmartEmployeePortal.Domain.Entities;

namespace SmartEmployeePortal.Domain.Interfaces;

/// <summary>
/// Repository contract for Employee persistence operations.
/// Defined here in Domain so Application layer can depend on it without
/// knowing anything about EF Core or SQL Server.
/// The actual implementation lives in Infrastructure.
/// </summary>
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Employee> Items, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
    void Update(Employee employee);
    void Delete(Employee employee);
}
