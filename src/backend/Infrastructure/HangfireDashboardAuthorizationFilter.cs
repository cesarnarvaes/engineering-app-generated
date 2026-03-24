using Hangfire.Dashboard;
using System.Security.Claims;

namespace BizCrm.Backend;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow access in development mode
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        // In production, require authentication and specific roles
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Only allow system administrators and managers to access Hangfire dashboard
        var user = httpContext.User;
        return user.IsInRole("systemadmin") || user.IsInRole("manager");
    }
}