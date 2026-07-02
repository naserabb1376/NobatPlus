using Hangfire.Dashboard;

namespace NobatPlusAPI.Tools
{
    public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User?.Identity?.IsAuthenticated == true &&
                   httpContext.User.GetCurrentRoleId() == (long)NobatPlusDATA.Tools.DbTools.BaseRole.Admin;
        }
    }
}
