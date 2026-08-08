namespace SmartEmployeePortal.Application.Common.Interfaces;

public interface IBlobStorageService
{
    /// <summary>Returns a time-limited SAS upload URL and the permanent blob URL.</summary>
    Task<(string SasUploadUrl, string PermanentBlobUrl)> GenerateUploadSasTokenAsync(
        Guid employeeId, string fileExtension, CancellationToken cancellationToken = default);

    /// <summary>Wraps a stored permanent blob URL with a 1-hour read-only SAS — no network call needed.</summary>
    string GenerateReadSasUrl(string permanentBlobUrl);

    Task DeleteBlobAsync(string permanentBlobUrl, CancellationToken cancellationToken = default);
}
