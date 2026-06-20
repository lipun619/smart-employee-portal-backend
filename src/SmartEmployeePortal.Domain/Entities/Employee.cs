using SmartEmployeePortal.Domain.Common;
using SmartEmployeePortal.Domain.Enums;

namespace SmartEmployeePortal.Domain.Entities;

/// <summary>
/// The core domain entity for this application.
/// Contains all employee-related data and business rules.
///
/// Key design decisions:
/// - Inherits BaseEntity (Guid PK — prevents sequential ID attacks)
/// - Implements IAuditableEntity (full change tracking)
/// - Soft delete via IsDeleted flag (data is never permanently lost)
/// - ProfileImageUrl stored as string URL pointing to Azure Blob Storage (Phase 3)
/// </summary>
public class Employee : BaseEntity, IAuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly HireDate { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;
    public Gender Gender { get; set; } = Gender.PreferNotToSay;

    // FK to Department
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Placeholder for Phase 3: Azure Blob Storage image URL
    public string? ProfileImageUrl { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Domain computed property — no service dependency needed
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Domain method to mark an employee as terminated.
    /// Business logic lives in the entity, not in a handler or service.
    /// </summary>
    public void Terminate()
    {
        EmploymentStatus = EmploymentStatus.Terminated;
    }

    /// <summary>
    /// Soft delete — marks the record as deleted without removing it from DB.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
