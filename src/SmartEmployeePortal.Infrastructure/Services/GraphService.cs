using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using SmartEmployeePortal.Application.Common.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Services;

/// <summary>
/// Wraps Microsoft.Graph SDK calls. Uses DefaultAzureCredential:
///   - Azure: automatically uses the App Service System-assigned Managed Identity
///   - Local dev: falls back to Azure CLI credentials (run 'az login' once)
/// Requires the Managed Identity to have User.Read.All + ProfilePhoto.Read.All app roles.
/// </summary>
public class GraphService : IGraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GraphService> _logger;

    public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
    {
        _logger = logger;

        var tenantId = configuration["AzureAd:TenantId"]!;
        var clientId = configuration["AzureAd:ClientId"]!;
        var clientSecret = configuration["Graph:ClientSecret"];

        // Local dev: ClientSecretCredential uses the API app registration + a client secret.
        // Production: DefaultAzureCredential uses the App Service Managed Identity (no secret).
        // Both paths get an app-only token — same User.Read.All application permission applies.
        Azure.Core.TokenCredential credential = !string.IsNullOrWhiteSpace(clientSecret)
            ? new ClientSecretCredential(tenantId, clientId, clientSecret)
            : new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = tenantId });

        _graphClient = new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]);
    }

    public async Task<GraphUserProfile?> GetUserProfileByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        try
        {
            // OData filter — email is validated as an email address upstream so safe to embed,
            // but we still strip single quotes to guard against OData injection
            var safeEmail = email.Replace("'", "");

            var result = await _graphClient.Users.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"mail eq '{safeEmail}' or userPrincipalName eq '{safeEmail}'";
                config.QueryParameters.Select = ["id", "displayName", "jobTitle", "department"];
                config.Headers.Add("ConsistencyLevel", "eventual");
            }, cancellationToken);

            var user = result?.Value?.FirstOrDefault();
            if (user is null) return null;

            return new GraphUserProfile(user.Id!, user.DisplayName, user.JobTitle, user.Department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Graph API error looking up user by email {Email}", email);
            return null;
        }
    }

    public async Task<GraphUserProfile?> GetUserProfileByIdAsync(
        string entraObjectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _graphClient.Users[entraObjectId].GetAsync(config =>
            {
                config.QueryParameters.Select = ["id", "displayName", "jobTitle", "department"];
            }, cancellationToken);

            if (user is null) return null;

            return new GraphUserProfile(user.Id!, user.DisplayName, user.JobTitle, user.Department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Graph API error looking up user by Object ID {EntraObjectId}", entraObjectId);
            return null;
        }
    }

    public async Task<Stream?> GetUserPhotoStreamAsync(
        string entraObjectId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphClient.Users[entraObjectId].Photo.Content.GetAsync(
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Graph API could not retrieve photo for user {EntraObjectId}", entraObjectId);
            return null;
        }
    }
}
