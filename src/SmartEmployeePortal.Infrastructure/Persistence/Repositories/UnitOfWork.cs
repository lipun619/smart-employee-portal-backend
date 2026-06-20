using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work implementation.
/// Wraps all repositories under a single DbContext so they all share
/// the same database transaction. When SaveChangesAsync() is called,
/// ALL pending changes from all repositories commit together atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IEmployeeRepository Employees { get; }
    public IDepartmentRepository Departments { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository)
    {
        _context = context;
        Employees = employeeRepository;
        Departments = departmentRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
