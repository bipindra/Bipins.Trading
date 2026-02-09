using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TradingApp.Web.Infrastructure;

/// <summary>
/// Rejects paths that start with "api/" so the SPA fallback does not match API requests.
/// </summary>
public sealed class NotApiPathConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value is string path)
            return !path.StartsWith("api/", StringComparison.OrdinalIgnoreCase);
        return true;
    }
}
