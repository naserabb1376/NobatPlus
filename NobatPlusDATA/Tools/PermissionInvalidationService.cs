using NobatPlusDATA.DataLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Tools
{
    public interface IPermissionInvalidationService
    {
        Task BumpUserVersionAsync(List<long> userIds, CancellationToken ct = default);
        Task BumpRoleUsersVersionAsync(List<long> roleIds, CancellationToken ct = default);
    }

    public class PermissionInvalidationService : IPermissionInvalidationService
    {
        private readonly NobatPlusContext _db;

        public PermissionInvalidationService(NobatPlusContext db) => _db = db;

        public Task BumpUserVersionAsync(List<long> userIds, CancellationToken ct = default)
            => _db.Persons
                .Where(u => userIds.Contains(u.ID))
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.PermissionsVersion, u => u.PermissionsVersion + 1), ct);

        public Task BumpRoleUsersVersionAsync(List<long> roleIds, CancellationToken ct = default)
            => _db.Persons
                .Where(u => roleIds.Contains(u.RoleId))
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.PermissionsVersion, u => u.PermissionsVersion + 1), ct);
    }
}
