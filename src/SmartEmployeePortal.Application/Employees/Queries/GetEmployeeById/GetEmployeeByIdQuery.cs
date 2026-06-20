using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;

namespace SmartEmployeePortal.Application.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;
