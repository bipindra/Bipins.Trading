namespace TradingApp.Web.Infrastructure;

/// <summary>
/// Consistent error shape for API: 400 and 500 responses.
/// </summary>
public record ApiErrorResponse(string Message, string[]? Errors = null);
