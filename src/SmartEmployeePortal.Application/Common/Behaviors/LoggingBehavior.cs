using MediatR;
using Microsoft.Extensions.Logging;

namespace SmartEmployeePortal.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs every command/query with timing.
/// Automatically logs: request name, execution time, and any exceptions.
///
/// Flow: Request → LoggingBehavior → ValidationBehavior → Handler → Response
///
/// This gives us free performance monitoring with zero code duplication —
/// every single request is logged without touching individual handlers.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);

        var startTime = DateTime.UtcNow;
        try
        {
            var response = await next(cancellationToken);
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms", requestName, elapsed);

            return response;
        }
        catch (Exception ex)
        {
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex,
                "Error handling {RequestName} after {ElapsedMs}ms", requestName, elapsed);
            throw;
        }
    }
}
