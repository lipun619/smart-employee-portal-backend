using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdQueryHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorage;

    public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork, IBlobStorageService blobStorage)
    {
        _unitOfWork = unitOfWork;
        _blobStorage = blobStorage;
    }

    public async Task<EmployeeDto> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Employee), request.Id);

        var dto = employee.Adapt<EmployeeDto>();

        if (!string.IsNullOrEmpty(dto.ProfileImageUrl))
            dto.ProfileImageUrl = _blobStorage.GenerateReadSasUrl(dto.ProfileImageUrl);

        return dto;
    }
}
