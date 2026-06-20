using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Departments.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsQueryHandler
    : IRequestHandler<GetAllDepartmentsQuery, IEnumerable<DepartmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllDepartmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DepartmentDto>> Handle(
        GetAllDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var departments = await _unitOfWork.Departments.GetAllAsync(cancellationToken);
        return departments.Adapt<IEnumerable<DepartmentDto>>();
    }
}
