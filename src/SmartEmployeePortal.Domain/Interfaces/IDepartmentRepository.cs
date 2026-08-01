using SmartEmployeePortal.Domain.Entities;

namespace SmartEmployeePortal.Domain.Interfaces;

/// <summary>
/// Repository contract for Department persistence operations.
/// </summary>
public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Department department, CancellationToken cancellationToken = default);
}
