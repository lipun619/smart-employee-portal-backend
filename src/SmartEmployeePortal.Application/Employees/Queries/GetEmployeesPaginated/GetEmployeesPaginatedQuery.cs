using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;

namespace SmartEmployeePortal.Application.Employees.Queries.GetEmployeesPaginated;

/// <summary>
/// Paginated query with optional search.
/// Pagination prevents loading all records at once — critical for performance
/// when there are thousands of employees.
/// </summary>
public record GetEmployeesPaginatedQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<PaginatedEmployeesDto>;
