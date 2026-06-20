using MediatR;
using SmartEmployeePortal.Domain.Enums;

namespace SmartEmployeePortal.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Command to update an existing employee.
/// Id identifies which employee to update.
/// Uses record type — immutable by default which is ideal for commands.
/// </summary>
public record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    DateOnly HireDate,
    string? JobTitle,
    decimal? Salary,
    EmploymentStatus EmploymentStatus,
    Gender Gender,
    Guid DepartmentId
) : IRequest<Unit>; // Unit = MediatR's void — command updates but returns nothing meaningful
