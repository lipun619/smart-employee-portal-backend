using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartEmployeePortal.Domain.Entities;
using SmartEmployeePortal.Domain.Enums;

namespace SmartEmployeePortal.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Employee entity.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(e => e.JobTitle)
            .HasMaxLength(100);

        builder.Property(e => e.Salary)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(200);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(200);

        // Store enums as int in DB (compact), but Swagger will show string names via JSON converter
        builder.Property(e => e.EmploymentStatus)
            .HasConversion<int>();

        builder.Property(e => e.Gender)
            .HasConversion<int>();

        // Unique index on email — only enforce for non-deleted employees
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Ignore computed property — FullName is derived, not stored in DB
        builder.Ignore(e => e.FullName);

        // FK relationship: one Department has many Employees
        // DeleteBehavior.Restrict prevents deleting a department that has employees
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
