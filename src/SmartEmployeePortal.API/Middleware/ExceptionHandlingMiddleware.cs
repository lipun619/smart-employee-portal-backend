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

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";

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
                    _env.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later."))
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
