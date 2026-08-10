using MediatR;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Commands.SyncFromGraph;

public class SyncFromGraphCommandHandler : IRequestHandler<SyncFromGraphCommand, GraphSyncResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGraphService _graphService;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<SyncFromGraphCommandHandler> _logger;

    public SyncFromGraphCommandHandler(
        IUnitOfWork unitOfWork,
        IGraphService graphService,
        IBlobStorageService blobStorage,
        ILogger<SyncFromGraphCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _graphService = graphService;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<GraphSyncResultDto> Handle(
        SyncFromGraphCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Employee), request.EmployeeId);

        // Resolve the Entra profile: use cached Object ID if we have it; fall back to email lookup
        GraphUserProfile? profile = null;

        if (!string.IsNullOrWhiteSpace(employee.EntraObjectId))
        {
            profile = await _graphService.GetUserProfileByIdAsync(employee.EntraObjectId, cancellationToken);
        }

        if (profile is null)
        {
            profile = await _graphService.GetUserProfileByEmailAsync(employee.Email, cancellationToken);
        }

        if (profile is null)
        {
            _logger.LogWarning(
                "Graph sync skipped for Employee {EmployeeId}: no matching Entra user found for email {Email}",
                employee.Id, employee.Email);

            return new GraphSyncResultDto
            {
                EmployeeId = employee.Id,
                Message = $"No Entra user found matching email '{employee.Email}'. Ensure the employee has an account in this tenant."
            };
        }

        // Cache the Entra Object ID for future syncs
        employee.EntraObjectId = profile.EntraObjectId;

        // Sync display name → split on first space; keep existing values if Graph returns empty
        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            var parts = profile.DisplayName.Trim().Split(' ', 2);
            employee.FirstName = parts[0];
            employee.LastName = parts.Length > 1 ? parts[1] : employee.LastName;
        }

        if (!string.IsNullOrWhiteSpace(profile.JobTitle))
            employee.JobTitle = profile.JobTitle;

        // Sync profile photo — upload the stream from Graph directly into Blob Storage
        var photoSynced = false;
        try
        {
            var photoStream = await _graphService.GetUserPhotoStreamAsync(profile.EntraObjectId, cancellationToken);
            if (photoStream is not null)
            {
                await using (photoStream)
                {
                    var blobUrl = await _blobStorage.UploadStreamAsync(
                        employee.Id, photoStream, "image/jpeg", ".jpg", cancellationToken);

                    employee.ProfileImageUrl = blobUrl;
                    photoSynced = true;
                }
            }
        }
        catch (Exception ex)
        {
            // Photo is optional — log and continue without failing the whole sync
            _logger.LogWarning(ex, "Graph photo sync failed for Employee {EmployeeId}; profile fields still updated", employee.Id);
        }

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Graph sync completed for Employee {EmployeeId}. PhotoSynced={PhotoSynced}",
            employee.Id, photoSynced);

        return new GraphSyncResultDto
        {
            EmployeeId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            JobTitle = employee.JobTitle,
            PhotoSynced = photoSynced,
            Message = "Sync completed successfully."
        };
    }
}
