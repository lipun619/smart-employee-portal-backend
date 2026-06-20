using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdQueryHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Employee), request.Id);

        return employee.Adapt<EmployeeDto>();
    }
}
