using SmartEmployeePortal.Domain.Common;

namespace SmartEmployeePortal.Domain.Interfaces;

/// <summary>
/// Repository contract for Department persistence operations.
/// </summary>
public interface IDepartmentRepository
{
    Task<IEnumerable<Domain.Entities.Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Department department, CancellationToken cancellationToken = default);
}
