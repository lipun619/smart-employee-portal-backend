using FluentValidation;

namespace SmartEmployeePortal.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// FluentValidation validator for CreateEmployeeCommand.
/// Runs automatically via ValidationBehavior before the handler executes.
/// Rules here are purely structural — e.g., "email must be valid format".
/// Business rules (e.g., "email must be unique") stay in the handler.
/// </summary>
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Hire date cannot be in the future.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-16)))
            .WithMessage("Employee must be at least 16 years old.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than zero.")
            .When(x => x.Salary.HasValue);

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required.");

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("Job title must not exceed 100 characters.")
            .When(x => x.JobTitle is not null);
    }
}
