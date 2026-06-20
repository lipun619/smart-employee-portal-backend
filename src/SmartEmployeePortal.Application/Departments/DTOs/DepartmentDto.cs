using SmartEmployeePortal.Domain.Common;

namespace SmartEmployeePortal.Application.Departments.DTOs;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
