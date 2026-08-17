using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Interfaces;

namespace SmartEmployeePortal.Infrastructure.Services.Email;

/// <summary>
/// Azure Communication Services implementation of IEmailService.
/// Uses the ACS Email SDK to deliver transactional email (birthday / onboarding notifications).
/// Connection string and sender address are read from configuration / Key Vault at runtime.
/// </summary>
public class AcsEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;
    private readonly ILogger<AcsEmailService> _logger;

    public AcsEmailService(IConfiguration configuration, ILogger<AcsEmailService> logger)
    {
        _logger = logger;

        var connectionString = configuration["ACS:ConnectionString"]
            ?? configuration["ACS--ConnectionString"]
            ?? throw new InvalidOperationException(
                "AzureCommunicationServices:ConnectionString is required. " +
                "Configure it in appsettings, environment variables, or Key Vault.");

        _senderAddress = configuration["ACS:SenderAddress"]
            ?? configuration["ACS--SenderAddress"]
            ?? throw new InvalidOperationException(
                "AzureCommunicationServices:SenderAddress is required.");

        _emailClient = new EmailClient(connectionString);
    }

    public async Task SendAsync(
        string toAddress,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new EmailMessage(
                senderAddress: _senderAddress,
                recipients: new EmailRecipients(new[] { new EmailAddress(toAddress) }),
                content: new EmailContent(subject) { Html = htmlBody });

            // WaitUntil.Started returns immediately after the send is accepted by ACS.
            // Use WaitUntil.Completed if you need delivery confirmation (slower).
            var operation = await _emailClient.SendAsync(
                WaitUntil.Started, message, cancellationToken);

            _logger.LogInformation(
                "Email accepted by ACS — recipient: {Recipient}, operationId: {OperationId}",
                toAddress, operation.Id);
        }
        catch (RequestFailedException ex) when (ex.Status == 429)
        {
            var retryAfterHeader = ex.GetRawResponse()?.Headers.TryGetValue("Retry-After", out var retryAfter)
                == true
                ? retryAfter.ToString()
                : "unknown";

            _logger.LogWarning(
                ex,
                "ACS rate limit hit while sending email to {Recipient}. Retry-After: {RetryAfter}. No immediate retry will be performed.",
                toAddress,
                retryAfterHeader);

            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} — subject: {Subject}", toAddress, subject);
            throw;
        }
    }
}
