using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartEmployeePortal.API.Middleware;
using SmartEmployeePortal.Application;
using SmartEmployeePortal.Infrastructure;
using SmartEmployeePortal.Infrastructure.Persistence;

// ============================================================
// STEP 1: Configure Serilog early so startup errors are logged
// ============================================================
SmartEmployeePortal.Infrastructure.DependencyInjection.ConfigureSerilog(
    new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build());

Log.Information("Starting Smart Employee Portal API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // ============================================================
    // STEP 2: Load secrets from Azure Key Vault (skip if not configured)
    // ============================================================
    var keyVaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
    if (!string.IsNullOrWhiteSpace(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
        Log.Information("Azure Key Vault configured: {VaultUri}", keyVaultUri);
    }
    else
    {
        Log.Warning("Azure Key Vault URI not configured. Using appsettings for secrets (dev only).");
    }

    // ============================================================
    // STEP 3: Register Application + Infrastructure services
    // ============================================================
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Application Insights — only activate when connection string is configured
    // In local dev without Key Vault, this is safely skipped
    var appInsightsConnection = builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    if (!string.IsNullOrWhiteSpace(appInsightsConnection))
    {
        builder.Services.AddApplicationInsightsTelemetry(options =>
            options.ConnectionString = appInsightsConnection);
        Log.Information("Application Insights configured.");
    }
    else
    {
        Log.Warning("Application Insights connection string not configured. Telemetry disabled for local dev.");
    }

    // ============================================================
    // STEP 4: Controllers with JSON enum string conversion
    // ============================================================
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    // ============================================================
    // STEP 5: Swagger
    // ============================================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Smart Employee Portal API",
            Version = "v1",
            Description = "Enterprise Employee Management System"
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Format: Bearer {token}"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ============================================================
    // STEP 6: CORS
    // ============================================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AngularPolicy", policy =>
            policy.AllowAnyMethod()
                  .AllowAnyHeader());
    });

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.RequireHttpsMetadata = false;
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // ============================================================
    // STEP 7: Middleware pipeline
    // ============================================================
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Employee Portal API v1");
        c.RoutePrefix = string.Empty; // Swagger at root: http://localhost:5100/
    });

    app.UseHttpsRedirection();
    app.UseCors("AngularPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging();
    app.MapControllers();

    // ============================================================
    // STEP 8: Auto-apply EF Core migrations (opt-in outside Development)
    // ============================================================
    var runMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");

    if (runMigrationsOnStartup)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Log.Information("Applying database migrations...");
        await db.Database.MigrateAsync();
        Log.Information("Migrations applied successfully.");
    }
    else
    {
        Log.Information("Skipping automatic database migrations on startup.");
    }

    Log.Information("API started successfully on http://localhost:5048");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
