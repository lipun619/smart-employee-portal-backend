namespace SmartEmployeePortal.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Uses Guid as PK to prevent sequential ID enumeration attacks (IDOR).
/// Every entity gets an Id automatically — no manual assignment needed.
/// </summary>
public abstract class BaseEntity
{
    // 'init' allows setting in object initializers (e.g., EF Core seed data)
    // but prevents mutation after construction — immutable after creation
    public Guid Id { get; init; } = Guid.NewGuid();
}
