using Microsoft.EntityFrameworkCore;
using MTPermissionCenter.Abstractions;
using NobatPlusDATA.DataLayer;

namespace NobatPlusAPI.Tools
{
    public sealed class ActiveProfileUserRoleProvider : IUserRoleProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NobatPlusContext _context;

        public ActiveProfileUserRoleProvider(
            IHttpContextAccessor httpContextAccessor,
            NobatPlusContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<long?> GetUserRoleIdAsync(
            long userId,
            CancellationToken ct = default)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true &&
                user.GetCurrentUserId() == userId)
            {
                var activeRoleId = user.GetCurrentRoleId();
                if (activeRoleId > 0)
                    return activeRoleId;
            }

            return await _context.Persons
                .AsNoTracking()
                .Where(x => x.ID == userId)
                .Select(x => (long?)x.RoleId)
                .SingleOrDefaultAsync(ct);
        }
    }
}
