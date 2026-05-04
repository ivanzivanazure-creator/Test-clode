using AccountingERP.Domain.Exceptions;
using System.Text.Json;

namespace AccountingERP.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            DomainException     => (StatusCodes.Status400BadRequest,   "Greška domenskog pravila"),
            NotFoundException   => (StatusCodes.Status404NotFound,     "Nije pronađeno"),
            BusinessException b => (StatusCodes.Status422UnprocessableEntity, b.Code),
            _                  => (StatusCodes.Status500InternalServerError,  "Interna greška servera"),
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = status;

        var problem = new
        {
            type   = $"https://httpstatuses.com/{status}",
            title,
            status,
            detail = exception.Message,
            traceId = context.TraceIdentifier,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
