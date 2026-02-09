using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TradingApp.Web.Infrastructure;

/// <summary>
/// Rejects the literal "search" so GET api/Stocks/search matches only the Search action.
/// </summary>
public sealed class NotSearchRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value is string s)
            return !string.Equals(s, "search", StringComparison.OrdinalIgnoreCase);
        return false;
    }
}
