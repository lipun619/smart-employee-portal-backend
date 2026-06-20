using FluentValidation;

namespace SmartEmployeePortal.Application.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required.");

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
    }
}
