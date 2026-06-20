using MediatR;
using SmartEmployeePortal.Application.Departments.DTOs;

namespace SmartEmployeePortal.Application.Departments.Queries.GetAllDepartments;

public record GetAllDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>;
