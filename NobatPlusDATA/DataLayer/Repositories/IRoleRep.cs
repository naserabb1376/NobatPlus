using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IRoleRep
    {
        Task<ListResultObject<Role>> GetAllRolesAsync(long permissionId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");

        Task<RowResultObject<Role>> GetRoleByIdAsync(long roleId);

        Task<BitResultObject> AddRoleAsync(Role role);

        Task<BitResultObject> EditRoleAsync(Role role);

        Task<BitResultObject> RemoveRoleAsync(Role role);

        Task<BitResultObject> RemoveRoleAsync(long roleId);

        Task<BitResultObject> ExistRoleAsync(long roleId);
    }
}