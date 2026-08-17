using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using SmartEmployeePortal.Application.Common.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorage:ConnectionString"]
            ?? configuration["BlobStorage--ConnectionString"]
            ?? configuration["BlobStorage__ConnectionString"];

        _containerName = configuration["BlobStorage:ContainerName"] ?? "profile-photos";

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "BlobStorage:ConnectionString is not configured. " +
                "Add Key Vault secret 'BlobStorage--ConnectionString'.");

        _serviceClient = new BlobServiceClient(connectionString);
    }

    public async Task<(string SasUploadUrl, string PermanentBlobUrl)> GenerateUploadSasTokenAsync(
        Guid employeeId, string fileExtension, CancellationToken cancellationToken = default)
    {
        // Scoped to employee ID — prevents any cross-employee path traversal
        var blobName = $"{employeeId}/{Guid.NewGuid()}{fileExtension}";
        var containerClient = _serviceClient.GetBlobContainerClient(_containerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobName);

        // Write + Create only — no read/delete, expires in 5 minutes
        var sasUri = blobClient.GenerateSasUri(
            BlobSasPermissions.Write | BlobSasPermissions.Create,
            DateTimeOffset.UtcNow.AddMinutes(5));

        return (sasUri.ToString(), blobClient.Uri.AbsoluteUri);
    }

    public string GenerateReadSasUrl(string permanentBlobUrl)
    {
        var uri = new Uri(permanentBlobUrl);
        var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
        if (segments.Length < 2) return permanentBlobUrl;

        var blobName = segments[1];
        var containerClient = _serviceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        // Read-only, 1-hour window — computed locally using the account key, no HTTP call made
        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
        return sasUri.ToString();
    }

    public async Task DeleteBlobAsync(string permanentBlobUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(permanentBlobUrl);
        // AbsolutePath = /{containerName}/{employeeId}/{filename} — skip container segment
        var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
        if (segments.Length < 2) return;

        var blobName = segments[1];
        var containerClient = _serviceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<string> UploadStreamAsync(
        Guid employeeId, Stream content, string contentType, string fileExtension,
        CancellationToken cancellationToken = default)
    {
        var blobName = $"{employeeId}/{Guid.NewGuid()}{fileExtension}";
        var containerClient = _serviceClient.GetBlobContainerClient(_containerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        return blobClient.Uri.AbsoluteUri;
    }
}
