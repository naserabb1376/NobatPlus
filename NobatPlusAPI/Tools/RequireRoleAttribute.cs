using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NobatPlusAPI.Tools
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly long[] _roleIds;
        public IReadOnlyCollection<long> RoleIds => _roleIds;

        public RequireRoleAttribute(params long[] roleIds)
        {
            _roleIds = roleIds ?? Array.Empty<long>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roleId = context.HttpContext.User.GetCurrentRoleId();
            if (!_roleIds.Contains(roleId))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
