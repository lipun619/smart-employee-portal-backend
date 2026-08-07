namespace SmartEmployeePortal.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}
