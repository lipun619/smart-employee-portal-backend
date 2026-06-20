using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartEmployeePortal.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI tools (dotnet ef migrations add, etc.)
/// 
/// Why needed? When you run "dotnet ef migrations add", EF Core tools need to
/// instantiate the DbContext outside of the normal ASP.NET DI pipeline.
/// This factory tells it exactly how to build the DbContext using the
/// connection string from appsettings.Development.json.
/// 
/// This class is ONLY used by EF Core tooling — never at runtime.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Walk up from Infrastructure project to find the API's appsettings
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "SmartEmployeePortal.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in appsettings.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
