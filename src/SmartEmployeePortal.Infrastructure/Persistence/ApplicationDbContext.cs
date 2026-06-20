using Microsoft.EntityFrameworkCore;
using SmartEmployeePortal.Domain.Common;
using SmartEmployeePortal.Domain.Entities;
using SmartEmployeePortal.Infrastructure.Persistence.Configurations;

namespace SmartEmployeePortal.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context — the single entry point to the database.
///
/// Key features configured here:
/// 1. Global query filters: WHERE IsDeleted = 0 applied automatically to Employee + Department queries
/// 2. SaveChangesAsync override: auto-stamps audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
/// 3. Fluent API configurations: table names, constraints, indexes defined in separate configuration files
/// </summary>
public class ApplicationDbContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters — soft delete filter applied universally
        // Every LINQ query on Employees/Departments will automatically append:
        // WHERE IsDeleted = 0
        // No need to remember to filter in every repository method
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Intercepts every save operation to auto-populate audit fields.
    /// This means handlers never need to manually set CreatedAt/UpdatedAt.
    /// In Phase 2 (auth), CreatedBy/UpdatedBy will come from the JWT claims.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.CreatedBy ??= "system"; // Phase 2: replace with JWT claim
                    break;

                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Entity.UpdatedBy ??= "system"; // Phase 2: replace with JWT claim
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
