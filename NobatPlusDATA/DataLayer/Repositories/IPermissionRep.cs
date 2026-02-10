using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTPermissionCenter.EFCore.Entities;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPermissionRep
    {
        Task<ListResultObject<MTPermissionCenter_Permission>> GetAllPermissionsAsync(long roleId =0,string permissionType="",long MenuParentId = 0,string MenuIds = "", int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");

        Task<RowResultObject<MTPermissionCenter_Permission>> GetPermissionByIdAsync(long permissionId);

        Task<BitResultObject> AddPermissionAsync(MTPermissionCenter_Permission permission);

        Task<BitResultObject> EditPermissionAsync(MTPermissionCenter_Permission permission);

        Task<BitResultObject> RemovePermissionAsync(MTPermissionCenter_Permission permission);

        Task<BitResultObject> RemovePermissionAsync(long permissionId);

        Task<BitResultObject> ExistPermissionAsync(long permissionId);
    }
}