using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartEmployeePortal.Domain.Interfaces;
using SmartEmployeePortal.Infrastructure.Persistence;
using SmartEmployeePortal.Infrastructure.Persistence.Repositories;
using System.IO;

namespace SmartEmployeePortal.Infrastructure;

/// <summary>
/// Registers all Infrastructure layer services: EF Core, repositories, Serilog.
/// Called once from Program.cs: builder.Services.AddInfrastructure(config)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register EF Core with Azure SQL Server.
        // Accept multiple key formats because Azure App Service can surface connection strings
        // differently depending on where they are configured (App Settings vs Connection Strings).
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? configuration["ConnectionStrings__DefaultConnection"]
            ?? configuration["ConnectionStrings:ConnectionStrings__DefaultConnection"]
            ?? configuration["SQLCONNSTR_DefaultConnection"]
            ?? configuration["SQLAZURECONNSTR_DefaultConnection"]
            ?? configuration["CUSTOMCONNSTR_DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Last-resort discovery: pick any config entry whose key ends with/contains DefaultConnection.
            // This covers unusual Azure key shapes like SQLCONNSTR_ConnectionStrings__DefaultConnection.
            var discovered = configuration
                .AsEnumerable()
                .FirstOrDefault(kv =>
                    !string.IsNullOrWhiteSpace(kv.Value) &&
                    kv.Key.Contains("DefaultConnection", StringComparison.OrdinalIgnoreCase));

            connectionString = discovered.Value;

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                Log.Warning("Resolved DB connection string from discovered configuration key: {ConfigKey}", discovered.Key);
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Configure one of: ConnectionStrings:DefaultConnection, " +
                "ConnectionStrings__DefaultConnection, SQLCONNSTR_DefaultConnection, or Key Vault secret ConnectionStrings--DefaultConnection.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Retry logic for transient Azure SQL failures (network blips, throttling)
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // Register repositories as Scoped — new instance per HTTP request
        // Scoped is correct for EF Core because DbContext is also Scoped
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Configure Serilog with Console + File sinks.
    /// Application Insights telemetry is handled separately by Microsoft.ApplicationInsights.AspNetCore
    /// which integrates with ILogger automatically — no separate Serilog sink needed.
    /// </summary>
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        var appServiceHome = Environment.GetEnvironmentVariable("HOME");
        var appServiceLogPath = !string.IsNullOrWhiteSpace(appServiceHome)
            ? Path.Combine(appServiceHome, "LogFiles", "Application", "smart-employee-portal-.log")
            : null;

        if (!string.IsNullOrWhiteSpace(appServiceLogPath))
        {
            var appServiceLogDirectory = Path.GetDirectoryName(appServiceLogPath);
            if (!string.IsNullOrWhiteSpace(appServiceLogDirectory))
            {
                Directory.CreateDirectory(appServiceLogDirectory);
            }
        }

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/smart-employee-portal-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                buffered: false,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

        if (!string.IsNullOrWhiteSpace(appServiceLogPath))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.File(
                path: appServiceLogPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                buffered: false,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}
