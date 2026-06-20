using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartEmployeePortal.Domain.Interfaces;
using SmartEmployeePortal.Infrastructure.Persistence;
using SmartEmployeePortal.Infrastructure.Persistence.Repositories;

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
        // Register EF Core with Azure SQL Server
        // Connection string comes from Key Vault via configuration pipeline set up in Program.cs
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Ensure Key Vault is configured.");

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
        Log.Logger = new LoggerConfiguration()
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
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
