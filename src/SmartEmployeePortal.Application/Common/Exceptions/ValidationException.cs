using FluentValidation.Results;

namespace SmartEmployeePortal.Application.Common.Exceptions;

/// <summary>
/// Thrown by ValidationBehavior when a command fails FluentValidation rules.
/// Contains all validation failures so the API can return them all at once.
/// The API layer catches this and returns HTTP 422 (Unprocessable Entity).
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
