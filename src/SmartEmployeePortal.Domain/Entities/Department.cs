using SmartEmployeePortal.Domain.Common;

namespace SmartEmployeePortal.Domain.Entities;

/// <summary>
/// Department entity — a logical grouping for employees.
/// Implements IAuditableEntity so every change is tracked with timestamp + user.
/// </summary>
public class Department : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Soft delete — records are never physically removed from the DB.
    // This preserves audit history and prevents accidental data loss.
    public bool IsDeleted { get; set; } = false;

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation property — one department has many employees
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
