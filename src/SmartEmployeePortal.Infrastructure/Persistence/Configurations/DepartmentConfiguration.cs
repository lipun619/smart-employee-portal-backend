using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEmployeePortal.Domain.Entities;

namespace SmartEmployeePortal.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Department entity.
/// Defines table name, column types, constraints, and indexes.
/// Keeps the entity class clean — no EF Core data annotations in Domain layer.
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .HasMaxLength(200);

        builder.Property(d => d.UpdatedBy)
            .HasMaxLength(200);

        // Unique index on department name to prevent duplicates
        builder.HasIndex(d => d.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0"); // Only enforce uniqueness on active departments

        // Seed initial departments so the app has data on first run
        builder.HasData(
            new Department
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Engineering",
                Description = "Software development and technical operations",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Department
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Human Resources",
                Description = "People management and recruitment",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Department
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Finance",
                Description = "Financial planning and accounting",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Department
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Marketing",
                Description = "Brand management and growth",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
