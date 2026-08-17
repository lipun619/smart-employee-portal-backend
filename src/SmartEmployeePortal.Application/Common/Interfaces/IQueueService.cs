namespace SmartEmployeePortal.Application.Common.Interfaces;

/// <summary>
/// Abstraction for enqueuing messages to Azure Storage Queue.
/// Allows the Application layer to trigger async workflows without coupling to Azure SDK.
/// </summary>
public interface IQueueService
{
    Task EnqueueAsync(
        string queueName,
        string message,
        CancellationToken cancellationToken = default);
}
