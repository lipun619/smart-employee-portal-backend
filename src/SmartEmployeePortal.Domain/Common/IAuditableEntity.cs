namespace SmartEmployeePortal.Domain.Common;

/// <summary>
/// Interface for entities that need audit tracking.
/// Every write operation (create/update) stamps WHO did it and WHEN.
/// Infrastructure layer (EF Core SaveChanges override) will auto-populate these.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
