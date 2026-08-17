using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Application.Common.Models;
using SmartEmployeePortal.Domain.Enums;
using SmartEmployeePortal.Infrastructure.Persistence;
using System.Text.Json;

namespace SmartEmployeePortal.API.Services;

/// <summary>
/// Lightweight background service that runs within the API process.
/// Every hour it checks for employees hired today and enqueues an onboarding
/// task message to the 'employee-tasks' Azure Storage Queue, which is picked
/// up by the EmployeeOnboardingQueueFunction in Azure Functions.
///
/// In-memory tracking (_enqueuedThisSession) prevents duplicate messages
/// within a single process lifetime; on restart the function is idempotent
/// because the queue trigger won't re-send already-processed emails.
/// </summary>
public class EmployeeEventBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmployeeEventBackgroundService> _logger;
    private readonly HashSet<Guid> _enqueuedThisSession = new();

    private const string EmployeeTasksQueue = "employee-tasks";

    public EmployeeEventBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EmployeeEventBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmployeeEventBackgroundService started — checking every hour for new hires.");

        // Use PeriodicTimer so the loop yields cleanly when the token is cancelled
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        // Run once immediately on startup, then on each tick
        do
        {
            await CheckAndEnqueueNewHiresAsync(stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAndEnqueueNewHiresAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var newHires = await dbContext.Employees
                .Where(e =>
                    e.HireDate == today &&
                    e.EmploymentStatus == EmploymentStatus.Active &&
                    !_enqueuedThisSession.Contains(e.Id))
                .ToListAsync(ct);

            if (newHires.Count == 0)
            {
                _logger.LogDebug("No new hires found for {Date}", today);
                return;
            }

            _logger.LogInformation("Found {Count} new hire(s) for {Date} — enqueueing onboarding tasks", newHires.Count, today);

            foreach (var employee in newHires)
            {
                var payload = JsonSerializer.Serialize(new EmployeeTaskMessage(
                    EmployeeId: employee.Id,
                    FullName: employee.FullName,
                    Email: employee.Email,
                    TaskType: "Onboarding",
                    EnqueuedAtUtc: DateTime.UtcNow));

                await queueService.EnqueueAsync(EmployeeTasksQueue, payload, ct);
                _enqueuedThisSession.Add(employee.Id);

                _logger.LogInformation(
                    "Onboarding task enqueued for {EmployeeName} ({EmployeeId})",
                    employee.FullName, employee.Id);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — swallow silently
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EmployeeEventBackgroundService — will retry on next tick");
        }
    }
}
