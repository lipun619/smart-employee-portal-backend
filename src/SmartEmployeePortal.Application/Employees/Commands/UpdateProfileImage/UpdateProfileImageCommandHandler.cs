using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Domain.Entities;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandHandler : IRequestHandler<UpdateProfileImageCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileImageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new NotFoundException(nameof(Employee), request.EmployeeId);

        // Reject anything that isn't an HTTPS Azure blob URL to prevent open redirects
        if (!IsValidBlobUrl(request.BlobUrl))
            throw new ArgumentException("BlobUrl must be a valid HTTPS Azure Blob Storage URL.");

        employee.ProfileImageUrl = request.BlobUrl;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsValidBlobUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.Host.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase);
}
