using System.Net;
using System.Text.Json;

namespace TradingApp.Web.Infrastructure;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
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
            var message = ex.Message;
            if (_env.IsDevelopment() && ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                message = $"{message} ({ex.InnerException.Message})";
            _logger.LogError(ex, "Unhandled exception: {Message}", message);
            if (_env.IsDevelopment())
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var body = new ApiErrorResponse(message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
        }
    }
}
