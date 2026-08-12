using System.Net;
using System.Text.Json;
using TaskManagementTool.Application.Exceptions;

namespace TaskManagementTool.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        var (statusCode, message) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);
        }
        else if (exception is DuplicateEmailException)
        {
            _logger.LogWarning(
                "Handled exception: {ExceptionType}. Path: {Path}, Method: {Method}",
                exception.GetType().Name, context.Request.Path, context.Request.Method);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception: {ExceptionType}. Path: {Path}, Method: {Method}, Message: {Message}",
                exception.GetType().Name, context.Request.Path, context.Request.Method, message);
        }

        if (context.Response.HasStarted)
        {
            // If the response has already started, we can't modify it
            throw exception;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new
        {
            statusCode,
            message,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            DuplicateEmailException => ((int)HttpStatusCode.Conflict, exception.Message),
            WeakPasswordException => ((int)HttpStatusCode.BadRequest, exception.Message),
            InvalidCredentialsException => ((int)HttpStatusCode.Unauthorized, exception.Message),
            TaskNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, exception.Message),
            TaskAccessDeniedException => ((int)HttpStatusCode.Forbidden, exception.Message),
            InvalidTaskReferenceException => ((int)HttpStatusCode.BadRequest, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };
    }
}