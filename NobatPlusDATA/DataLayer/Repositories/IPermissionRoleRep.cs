using MTPermissionCenter.EFCore.Entities;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPermissionRoleRep
    {
        Task<ListResultObject<MTPermissionCenter_PermissionRole>> GetAllPermissionRolesAsync(long RoleId = 0, long PerrmissionId = 0,string permissionType="", int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");

        Task<RowResultObject<MTPermissionCenter_PermissionRole>> GetPermissionRoleByIdAsync(long PermissionRoleId);

        Task<BitResultObject> AddPermissionRolesAsync(List<MTPermissionCenter_PermissionRole> PermissionRoles);

        Task<BitResultObject> EditPermissionRolesAsync(List<MTPermissionCenter_PermissionRole> PermissionRoles);

        Task<BitResultObject> RemovePermissionRolesAsync(List<MTPermissionCenter_PermissionRole> PermissionRoles);

        Task<BitResultObject> RemovePermissionRolesAsync(List<long> PermissionRoleIds);

        Task<BitResultObject> ExistPermissionRoleAsync(long PermissionRoleId);
    }
}