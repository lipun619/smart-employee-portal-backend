using FluentValidation;
using MediatR;

namespace SmartEmployeePortal.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically runs FluentValidation
/// before EVERY command handler executes.
///
/// Flow: Request → ValidationBehavior → Handler → Response
///
/// If any validator fails, a ValidationException is thrown immediately —
/// the handler never even runs. This enforces "validate at the boundary"
/// so business logic always receives clean, validated data.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new Exceptions.ValidationException(failures);

        return await next(cancellationToken);
    }
}
