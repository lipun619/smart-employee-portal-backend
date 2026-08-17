namespace SmartEmployeePortal.Application.Common.Interfaces;

/// <summary>
/// Abstraction for sending transactional emails.
/// Decouples the Application layer from the concrete email provider (ACS / SendGrid / SMTP).
/// </summary>
public interface IEmailService
{
    Task SendAsync(
        string toAddress,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
