using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Services;

/// <summary>
/// Azure Storage Queue implementation of IQueueService.
/// Creates the target queue if it does not exist, then appends the JSON message.
/// Messages are sent as plain UTF-8 text (no base64 encoding) for easy inspection in the portal.
/// </summary>
public class QueueService : IQueueService
{
    private readonly string _storageConnectionString;
    private readonly ILogger<QueueService> _logger;

    public QueueService(IConfiguration configuration, ILogger<QueueService> logger)
    {
        _logger = logger;
        _storageConnectionString = configuration["Queue:ConnectionString"]
            ?? configuration["Queue--ConnectionString"]
            ?? configuration["Queue__ConnectionString"]
            ?? configuration["Queue-ConnectionString"]
            ?? configuration["BlobStorage:ConnectionString"]
            ?? configuration["BlobStorage--ConnectionString"]
            ?? configuration["BlobStorage__ConnectionString"]
            ?? throw new InvalidOperationException(
                "Queue:ConnectionString is required. " +
                "Configure it in local.settings.json, environment variables, or Key Vault.");
    }

    public async Task EnqueueAsync(
        string queueName,
        string message,
        CancellationToken cancellationToken = default)
    {
        var options = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.None };
        var queueClient = new QueueClient(_storageConnectionString, queueName, options);

        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await queueClient.SendMessageAsync(message, cancellationToken);

        _logger.LogInformation("Message enqueued to queue '{QueueName}'", queueName);
    }
}
