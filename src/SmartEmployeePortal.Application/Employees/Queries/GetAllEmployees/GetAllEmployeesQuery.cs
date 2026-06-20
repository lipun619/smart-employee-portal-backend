using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;

namespace SmartEmployeePortal.Application.Employees.Queries.GetAllEmployees;

/// <summary>
/// Query to retrieve all non-deleted employees.
/// Queries are read-only — they never change state.
/// </summary>
public record GetAllEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>;
