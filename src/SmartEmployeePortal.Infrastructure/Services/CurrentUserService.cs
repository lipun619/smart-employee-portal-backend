using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartEmployeePortal.Application.Common.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Services;

// Reads identity from the current HTTP request's JWT claims (Entra ID v2.0 token shape).
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    // Prefer object ID (stable across name/email changes) over subject claim
    public string? UserId =>
        User?.FindFirstValue("oid") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        User?.FindFirstValue("preferred_username") ?? User?.FindFirstValue(ClaimTypes.Email);

    public string? DisplayName =>
        User?.FindFirstValue("name") ?? User?.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles =>
        User?.Claims.Where(c => c.Type == "roles").Select(c => c.Value)
        ?? Enumerable.Empty<string>();
}
