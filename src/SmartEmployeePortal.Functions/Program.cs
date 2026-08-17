using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartEmployeePortal.Application.Common.Interfaces;
using SmartEmployeePortal.Domain.Interfaces;
using SmartEmployeePortal.Infrastructure.Persistence;
using SmartEmployeePortal.Infrastructure.Persistence.Repositories;
using SmartEmployeePortal.Infrastructure.Services;
using SmartEmployeePortal.Infrastructure.Services.Email;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Load appsettings.json + appsettings.{env}.json (mirrors API project pattern)
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        config.AddJsonFile(
            $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
            optional: true,
            reloadOnChange: false);

        // Wire Key Vault when running in Azure (Managed Identity picks up automatically)
        var built = config.Build();
        var vaultUri = built["AzureKeyVault:VaultUri"];
        if (!string.IsNullOrWhiteSpace(vaultUri))
        {
            config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
        }
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // ── EF Core ────────────────────────────────────────────────────────────
        // Accept the same key shapes used by the API project and Azure App Settings
        var connectionString =
            config.GetConnectionString("DefaultConnection")
            ?? config["ConnectionString:DefaultConnection"]
            ?? config["ConnectionString--DefaultConnection"]
            ?? config["ConnectionStrings__DefaultConnection"]
            ?? config["ConnectionStrings:DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. " +
                "Set it in local.settings.json → ConnectionStrings section, " +
                "appsettings.Development.json, or as an Azure Function App Setting.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

        // ── Repositories ───────────────────────────────────────────────────────
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Email + Queue ──────────────────────────────────────────────────────
        services.AddScoped<IEmailService, AcsEmailService>();
        services.AddScoped<IQueueService, QueueService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    })
    .Build();

// ── Ensure required queues exist before host starts listening ──────────────
// The QueueTrigger binding polls but does not create the queue automatically.
// Prefer the dedicated queue connection string if configured; otherwise fall back
// to AzureWebJobsStorage, which is the default Function host storage account.
var config = host.Services.GetRequiredService<IConfiguration>();
var queueConnectionString =
    config["Queue:ConnectionString"]
    ?? config["Queue--ConnectionString"]
    ?? config["Queue__ConnectionString"]
    ?? config["Queue-ConnectionString"]
    ?? config["AzureWebJobsStorage"];

if (!string.IsNullOrWhiteSpace(queueConnectionString))
{
    var queueClient = new QueueClient(
        queueConnectionString,
        "employee-tasks",
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.None });
    await queueClient.CreateIfNotExistsAsync();
}

await host.RunAsync();
