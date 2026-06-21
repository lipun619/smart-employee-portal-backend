using System.Net;
using System.Text.Json;
using SmartEmployeePortal.API.Common;
using SmartEmployeePortal.Application.Common.Exceptions;
using ValidationException = SmartEmployeePortal.Application.Common.Exceptions.ValidationException;

namespace SmartEmployeePortal.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly bool _exposeDetailedErrors;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _env = env;
        _exposeDetailedErrors = _env.IsDevelopment()
            || configuration.GetValue<bool>("Diagnostics:ExposeDetailedErrors");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var requestPath = context.Request.Path.Value ?? "unknown";
        var requestMethod = context.Request.Method;

        _logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}. TraceId: {TraceId}. Message: {Message}",
            requestMethod,
            requestPath,
            traceId,
            exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Correlation-ID"] = traceId;

        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(ex.Message)),

            ValidationException ex => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse<object>.Fail("Validation failed.", ex.Errors)),

            InvalidOperationException ex => (
                HttpStatusCode.Conflict,
                ApiResponse<object>.Fail(ex.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(
                    _exposeDetailedErrors
                        ? exception.Message
                        : $"An unexpected error occurred. Please try again later. Reference ID: {traceId}"))
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
