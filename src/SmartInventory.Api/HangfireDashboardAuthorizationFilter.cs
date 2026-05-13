using System.Net;
using Hangfire.Dashboard;

namespace SmartInventory.Api;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.Connection.RemoteIpAddress != null
            && IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress);
    }
}
