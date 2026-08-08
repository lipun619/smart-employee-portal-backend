using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Entities;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Commands.GenerateSasToken;

public class GenerateSasTokenCommandHandler : IRequestHandler<GenerateSasTokenCommand, SasTokenDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorage;

    public GenerateSasTokenCommandHandler(IUnitOfWork unitOfWork, IBlobStorageService blobStorage)
    {
        _unitOfWork = unitOfWork;
        _blobStorage = blobStorage;
    }

    public async Task<SasTokenDto> Handle(GenerateSasTokenCommand request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new NotFoundException(nameof(Employee), request.EmployeeId);

        var (sasUploadUrl, permanentBlobUrl) = await _blobStorage.GenerateUploadSasTokenAsync(
            request.EmployeeId,
            request.FileExtension.ToLowerInvariant(),
            cancellationToken);

        return new SasTokenDto
        {
            SasUploadUrl = sasUploadUrl,
            PermanentBlobUrl = permanentBlobUrl
        };
    }
}
