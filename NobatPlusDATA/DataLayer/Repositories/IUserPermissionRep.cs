using MTPermissionCenter.EFCore.Entities;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IUserPermissionRep
    {
        Task<ListResultObject<MTPermissionCenter_UserPermission>> GetAllUserPermissionsAsync(long UserId = 0, long PerrmissionId = 0,string permissionType="", int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");

        Task<RowResultObject<MTPermissionCenter_UserPermission>> GetUserPermissionByIdAsync(long UserPermissionId);

        Task<BitResultObject> AddUserPermissionsAsync(List<MTPermissionCenter_UserPermission> UserPermissions);

        Task<BitResultObject> EditUserPermissionsAsync(List<MTPermissionCenter_UserPermission> UserPermissions);

        Task<BitResultObject> RemoveUserPermissionsAsync(List<MTPermissionCenter_UserPermission> UserPermissions);

        Task<BitResultObject> RemoveUserPermissionsAsync(List<long> UserPermissionIds);

        Task<BitResultObject> ExistUserPermissionAsync(long UserPermissionId);
    }
}