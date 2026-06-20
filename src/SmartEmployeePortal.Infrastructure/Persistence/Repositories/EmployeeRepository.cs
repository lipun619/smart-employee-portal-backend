using Microsoft.EntityFrameworkCore;
using SmartEmployeePortal.Domain.Entities;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Persistence.Repositories;

/// <summary>
/// Concrete implementation of IEmployeeRepository using EF Core.
/// Note: we don't need to filter IsDeleted manually — the global query
/// filter in ApplicationDbContext does it automatically for all queries.
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Department) // Eager load department for DepartmentName in DTO
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .AsQueryable();

        // Apply search filter if provided — searches across name, email, and job title
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                (e.JobTitle != null && e.JobTitle.ToLower().Contains(term)));
        }

        // Get total count BEFORE pagination (needed for frontend pager)
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AnyAsync(e => e.Email == email && (excludeId == null || e.Id != excludeId),
                cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
    }

    public void Update(Employee employee)
    {
        // EF Core tracks changes automatically — just mark as modified
        _context.Employees.Update(employee);
    }

    public void Delete(Employee employee)
    {
        // We use soft delete via employee.SoftDelete() in the handler,
        // then call Update — we never call Remove() here
        _context.Employees.Update(employee);
    }
}
