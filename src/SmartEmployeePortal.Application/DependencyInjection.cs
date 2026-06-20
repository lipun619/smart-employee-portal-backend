using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartEmployeePortal.Application.Common.Behaviors;

namespace SmartEmployeePortal.Application;

/// <summary>
/// Extension method that registers all Application layer services into the DI container.
/// Called once from Program.cs: builder.Services.AddApplication()
///
/// What gets registered:
/// - MediatR: scans this assembly for all IRequestHandler implementations
/// - FluentValidation: scans this assembly for all AbstractValidator implementations
/// - Pipeline behaviors: ValidationBehavior + LoggingBehavior run on every MediatR request
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register all handlers (Commands + Queries) found in this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Register pipeline behaviors — order matters:
            // LoggingBehavior runs first (outermost), then ValidationBehavior, then Handler
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register all validators found in this assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
