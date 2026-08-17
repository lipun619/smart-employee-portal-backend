namespace SmartEmployeePortal.Application.Common.Models;

/// <summary>
/// Shared message contract written to the Azure Storage Queue by the API
/// and consumed by the Azure Functions Queue trigger.
/// </summary>
public record EmployeeTaskMessage(
    Guid EmployeeId,
    string FullName,
    string Email,
    string TaskType,      // "Onboarding" | "Offboarding"
    DateTime EnqueuedAtUtc);
