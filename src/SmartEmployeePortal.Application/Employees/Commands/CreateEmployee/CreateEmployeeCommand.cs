using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Enums;

namespace SmartEmployeePortal.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// Command = an intention to change state.
/// IRequest<EmployeeDto> means: "when handled, return an EmployeeDto".
/// MediatR routes this command to its matching handler automatically.
/// </summary>
public record CreateEmployeeCommand(
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
) : IRequest<EmployeeDto>;
