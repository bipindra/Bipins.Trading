using Microsoft.AspNetCore.SignalR;

namespace TradingApp.Web.Hubs;

public sealed class ActivityLogHub : Hub
{
    // Simple hub for broadcasting activity logs to all connected clients
    // No subscription management needed - all logs are broadcast to everyone
}
