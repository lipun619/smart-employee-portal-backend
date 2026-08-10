namespace SmartEmployeePortal.Application.Common.Interfaces;

public interface IGraphService
{
    /// <summary>Looks up a user in Entra by email/UPN. Returns null if not found.</summary>
    Task<GraphUserProfile?> GetUserProfileByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Looks up a user in Entra by their Object ID. Returns null if not found.</summary>
    Task<GraphUserProfile?> GetUserProfileByIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

    /// <summary>Returns a stream of the user's profile photo, or null if none is set.</summary>
    Task<Stream?> GetUserPhotoStreamAsync(string entraObjectId, CancellationToken cancellationToken = default);
}

public sealed record GraphUserProfile(
    string EntraObjectId,
    string? DisplayName,
    string? JobTitle,
    string? Department
);
