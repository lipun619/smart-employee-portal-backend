using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Queries.GetAllEmployees;

public class GetAllEmployeesQueryHandler
    : IRequestHandler<GetAllEmployeesQuery, IEnumerable<EmployeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEmployeesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EmployeeDto>> Handle(
        GetAllEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(cancellationToken);

        // Mapster maps the collection in one line — no manual property mapping
        return employees.Adapt<IEnumerable<EmployeeDto>>();
    }
}
