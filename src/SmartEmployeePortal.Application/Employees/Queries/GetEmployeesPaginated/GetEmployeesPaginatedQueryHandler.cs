using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Queries.GetEmployeesPaginated;

public class GetEmployeesPaginatedQueryHandler
    : IRequestHandler<GetEmployeesPaginatedQuery, PaginatedEmployeesDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeesPaginatedQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedEmployeesDto> Handle(
        GetEmployeesPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Employees.GetPaginatedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            cancellationToken);

        return new PaginatedEmployeesDto
        {
            Items = items.Adapt<IEnumerable<EmployeeDto>>(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
