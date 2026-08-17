using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Domain.Enums;
using SmartEmployeePortal.Infrastructure.Persistence;

namespace SmartEmployeePortal.Functions.Functions;

/// <summary>
/// Runs daily at 08:00 UTC to check for employee birthdays and work anniversaries.
/// Sends personalised congratulatory emails directly via IEmailService (no queue hop needed —
/// the notification is fire-and-forget and doesn't need retry semantics beyond ACS).
/// </summary>
public class BirthdayAnniversaryTimerFunction
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<BirthdayAnniversaryTimerFunction> _logger;

    public BirthdayAnniversaryTimerFunction(
        ApplicationDbContext dbContext,
        IEmailService emailService,
        ILogger<BirthdayAnniversaryTimerFunction> logger)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _logger = logger;
    }

    // "0 0 8 * * *" = every day at 08:00:00 UTC
    [Function(nameof(BirthdayAnniversaryTimerFunction))]
    public async Task Run(
        [TimerTrigger("0 0 8 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _logger.LogInformation("BirthdayAnniversaryTimerFunction fired — checking for {Date}", today);

        await SendBirthdayEmailsAsync(today, cancellationToken);
        await SendAnniversaryEmailsAsync(today, cancellationToken);
    }

    // ── Birthday notifications ────────────────────────────────────────────────

    private async Task SendBirthdayEmailsAsync(DateOnly today, CancellationToken ct)
    {
        var employees = await _dbContext.Employees
            .Where(e =>
                e.EmploymentStatus == EmploymentStatus.Active &&
                e.DateOfBirth != null &&
                e.DateOfBirth.Value.Month == today.Month &&
                e.DateOfBirth.Value.Day == today.Day)
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} birthday(s) today", employees.Count);

        foreach (var employee in employees)
        {
            try
            {
                var subject = $"Happy Birthday, {employee.FirstName}! 🎂";
                var html = BuildBirthdayEmailHtml(employee.FirstName, employee.FullName);
                await _emailService.SendAsync(employee.Email, subject, html, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send birthday email to {Employee}", employee.FullName);
                // Continue processing remaining employees — don't let one failure block all
            }
        }
    }

    // ── Anniversary notifications ─────────────────────────────────────────────

    private async Task SendAnniversaryEmailsAsync(DateOnly today, CancellationToken ct)
    {
        var employees = await _dbContext.Employees
            .Where(e =>
                e.EmploymentStatus == EmploymentStatus.Active &&
                e.HireDate.Month == today.Month &&
                e.HireDate.Day == today.Day &&
                e.HireDate.Year != today.Year) // exclude same-year (first day isn't an anniversary)
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} work anniversary(s) today", employees.Count);

        foreach (var employee in employees)
        {
            try
            {
                var years = today.Year - employee.HireDate.Year;
                var subject = $"Happy {years}-Year Work Anniversary, {employee.FirstName}! 🎉";
                var html = BuildAnniversaryEmailHtml(employee.FirstName, employee.FullName, years);
                await _emailService.SendAsync(employee.Email, subject, html, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send anniversary email to {Employee}", employee.FullName);
            }
        }
    }

    // ── Email templates ───────────────────────────────────────────────────────

    private static string BuildBirthdayEmailHtml(string firstName, string fullName) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Segoe UI, Arial, sans-serif; background: #f4f4f4; padding: 32px;">
          <div style="max-width: 560px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px;">
            <h2 style="color: #0078d4;">Happy Birthday, {firstName}! 🎂</h2>
            <p>Hi {fullName},</p>
            <p>The whole team at <strong>Smart Employee Portal</strong> wishes you a wonderful birthday!</p>
            <p>We hope today is full of joy, celebration, and everything you deserve.</p>
            <br/>
            <p style="color: #555; font-size: 12px;">— Smart Employee Portal Team</p>
          </div>
        </body>
        </html>
        """;

    private static string BuildAnniversaryEmailHtml(string firstName, string fullName, int years) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Segoe UI, Arial, sans-serif; background: #f4f4f4; padding: 32px;">
          <div style="max-width: 560px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px;">
            <h2 style="color: #0078d4;">Happy {years}-Year Work Anniversary, {firstName}! 🎉</h2>
            <p>Hi {fullName},</p>
            <p>Today marks <strong>{years} year{(years == 1 ? "" : "s")}</strong> since you joined us at
            <strong>Smart Employee Portal</strong>.</p>
            <p>Thank you for your dedication, hard work, and everything you contribute to the team.
            Here's to many more successful years together!</p>
            <br/>
            <p style="color: #555; font-size: 12px;">— Smart Employee Portal Team</p>
          </div>
        </body>
        </html>
        """;
}
