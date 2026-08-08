using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Queries.GetEmployeesPaginated;

public class GetEmployeesPaginatedQueryHandler
    : IRequestHandler<GetEmployeesPaginatedQuery, PaginatedEmployeesDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorage;

    public GetEmployeesPaginatedQueryHandler(IUnitOfWork unitOfWork, IBlobStorageService blobStorage)
    {
        _unitOfWork = unitOfWork;
        _blobStorage = blobStorage;
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

        var dtos = items.Adapt<List<EmployeeDto>>();

        foreach (var dto in dtos.Where(d => !string.IsNullOrEmpty(d.ProfileImageUrl)))
            dto.ProfileImageUrl = _blobStorage.GenerateReadSasUrl(dto.ProfileImageUrl!);

        return new PaginatedEmployeesDto
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
