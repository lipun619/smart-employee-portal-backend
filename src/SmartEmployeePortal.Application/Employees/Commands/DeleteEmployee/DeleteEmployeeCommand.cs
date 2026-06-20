using MediatR;

namespace SmartEmployeePortal.Application.Employees.Commands.DeleteEmployee;

/// <summary>
/// Command to soft-delete an employee by ID.
/// We never hard-delete employees — this preserves audit history.
/// </summary>
public record DeleteEmployeeCommand(Guid Id) : IRequest<Unit>;
