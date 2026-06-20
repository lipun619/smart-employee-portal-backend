using SmartEmployeePortal.Domain.Enums;

namespace SmartEmployeePortal.Application.Employees.DTOs;

/// <summary>
/// DTO returned to the API consumer for read operations.
/// Notice: we return enum names as strings (not ints) for better readability.
/// We never return the raw Employee entity — that would expose internal fields.
/// </summary>
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly HireDate { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
    public string EmploymentStatus { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new employee — only fields the client needs to supply.
/// Id, CreatedAt, etc. are set server-side — client should NOT send them.
/// </summary>
public class CreateEmployeeDto
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
    public Guid DepartmentId { get; set; }
}

/// <summary>
/// DTO for updating an existing employee.
/// All fields are optional — only provided fields are updated (handled in the handler).
/// </summary>
public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly HireDate { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public Gender Gender { get; set; }
    public Guid DepartmentId { get; set; }
}

/// <summary>
/// Paginated list result wrapper used by GetEmployeesPaginated query.
/// </summary>
public class PaginatedEmployeesDto
{
    public IEnumerable<EmployeeDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
