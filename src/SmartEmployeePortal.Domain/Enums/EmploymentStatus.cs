namespace SmartEmployeePortal.Domain.Enums;

/// <summary>
/// Represents the current employment state of an employee.
/// Stored as int in the database for performance; displayed as string in API responses.
/// </summary>
public enum EmploymentStatus
{
    Active = 1,
    Inactive = 2,
    OnLeave = 3,
    Terminated = 4
}
