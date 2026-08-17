using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Application.Common.Models;
using System.Text.Json;

namespace SmartEmployeePortal.Functions.Functions;

/// <summary>
/// Triggered by a message on the 'employee-tasks' Azure Storage Queue.
/// Each message carries an EmployeeTaskMessage JSON payload (produced by the API's
/// EmployeeEventBackgroundService when a new hire or termination is recorded).
/// Sends the appropriate welcome / departure email via IEmailService.
/// </summary>
public class EmployeeOnboardingQueueFunction
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmployeeOnboardingQueueFunction> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmployeeOnboardingQueueFunction(
        IEmailService emailService,
        ILogger<EmployeeOnboardingQueueFunction> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    // Connection = "AzureWebJobsStorage" picks up the storage account configured in
    // local.settings.json (dev) or the Function App Settings (Azure).
    [Function(nameof(EmployeeOnboardingQueueFunction))]
    public async Task Run(
        [QueueTrigger("employee-tasks", Connection = "AzureWebJobsStorage")] string messageBody,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing employee task message: {Body}", messageBody);

        EmployeeTaskMessage? message;
        try
        {
            message = DeserializeMessage(messageBody);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Could not deserialize queue message — skipping poison message");
            return; // returning without throwing prevents infinite retry on a permanently bad message
        }

        if (message is null)
        {
            _logger.LogWarning("Deserialized message was null — skipping");
            return;
        }

        try
        {
            switch (message.TaskType)
            {
                case "Onboarding":
                    await HandleOnboardingAsync(message, cancellationToken);
                    break;

                case "Offboarding":
                    await HandleOffboardingAsync(message, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unknown task type '{TaskType}' for employee {EmployeeId}", message.TaskType, message.EmployeeId);
                    break;
            }
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 429)
        {
            _logger.LogWarning(
                ex,
                "ACS throttling while processing queue message for employee {EmployeeId}. Message will be left without immediate retry.",
                message.EmployeeId);
            return;
        }
    }

    private static EmployeeTaskMessage? DeserializeMessage(string messageBody)
    {
        try
        {
            return JsonSerializer.Deserialize<EmployeeTaskMessage>(messageBody, _jsonOptions);
        }
        catch (JsonException)
        {
            // Some queue messages can arrive as base64-encoded text depending on how they were written.
            // Handle that gracefully so older queued payloads do not poison the function.
            var decoded = TryDecodeBase64(messageBody);
            if (decoded is null)
            {
                throw;
            }

            return JsonSerializer.Deserialize<EmployeeTaskMessage>(decoded, _jsonOptions);
        }
    }

    private static string? TryDecodeBase64(string value)
    {
        try
        {
            var buffer = Convert.FromBase64String(value);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }
        catch
        {
            return null;
        }
    }

    // ── Onboarding ─────────────────────────────────────────────────────────────

    private async Task HandleOnboardingAsync(EmployeeTaskMessage message, CancellationToken ct)
    {
        _logger.LogInformation("Sending onboarding email to {Employee} ({Email})", message.FullName, message.Email);

        var subject = $"Welcome to Smart Employee Portal, {message.FullName.Split(' ')[0]}! 👋";
        var html = BuildOnboardingEmailHtml(message.FullName);

        await _emailService.SendAsync(message.Email, subject, html, ct);
        _logger.LogInformation("Onboarding email sent to {Email}", message.Email);
    }

    // ── Offboarding ────────────────────────────────────────────────────────────

    private async Task HandleOffboardingAsync(EmployeeTaskMessage message, CancellationToken ct)
    {
        _logger.LogInformation("Sending offboarding email to {Employee} ({Email})", message.FullName, message.Email);

        var subject = $"Farewell and best wishes, {message.FullName.Split(' ')[0]}";
        var html = BuildOffboardingEmailHtml(message.FullName);

        await _emailService.SendAsync(message.Email, subject, html, ct);
        _logger.LogInformation("Offboarding email sent to {Email}", message.Email);
    }

    // ── Email templates ────────────────────────────────────────────────────────

    private static string BuildOnboardingEmailHtml(string fullName) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Segoe UI, Arial, sans-serif; background: #f4f4f4; padding: 32px;">
          <div style="max-width: 560px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px;">
            <h2 style="color: #0078d4;">Welcome to the team, {fullName}! 👋</h2>
            <p>We are thrilled to have you join <strong>Smart Employee Portal</strong>.</p>
            <p>Here are a few things to get you started:</p>
            <ul>
              <li>Log in to the <a href="https://delightful-glacier-043e4031e.7.azurestaticapps.net">Smart Employee Portal</a></li>
              <li>Complete your profile and upload a profile photo</li>
              <li>Reach out to HR if you have any questions</li>
            </ul>
            <p>We look forward to working with you!</p>
            <br/>
            <p style="color: #555; font-size: 12px;">— Smart Employee Portal Team</p>
          </div>
        </body>
        </html>
        """;

    private static string BuildOffboardingEmailHtml(string fullName) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Segoe UI, Arial, sans-serif; background: #f4f4f4; padding: 32px;">
          <div style="max-width: 560px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px;">
            <h2 style="color: #0078d4;">Farewell, {fullName}</h2>
            <p>On behalf of everyone at <strong>Smart Employee Portal</strong>, we want to say
            thank you for your contributions and wish you all the best in your next chapter.</p>
            <p>Please ensure you have completed the offboarding checklist with HR.</p>
            <br/>
            <p style="color: #555; font-size: 12px;">— Smart Employee Portal Team</p>
          </div>
        </body>
        </html>
        """;
}
