using FluentValidation;

namespace SmartEmployeePortal.Application.Employees.Commands.GenerateSasToken;

public class GenerateSasTokenCommandValidator : AbstractValidator<GenerateSasTokenCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];

    public GenerateSasTokenCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.FileExtension)
            .NotEmpty()
            .Must(ext => AllowedExtensions.Contains(ext.ToLowerInvariant()))
            .WithMessage("Only .jpg, .jpeg, and .png images are allowed.");
    }
}
