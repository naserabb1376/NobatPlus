using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAdminRep
    {
        public Task<ListResultObject<Admin>> GetAllAdminsAsync(string role ="",int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ListResultObject<Admin>> GetAdminsOfDiscountAsync(long DiscountId, string role = "", int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<Admin>> GetAdminByIdAsync(long adminId);
        public Task<BitResultObject> AddAdminAsync(Admin admin);
        public Task<BitResultObject> EditAdminAsync(Admin admin);
        public Task<BitResultObject> RemoveAdminAsync(Admin admin);
        public Task<BitResultObject> RemoveAdminAsync(long adminId);
        public Task<BitResultObject> ExistAdminAsync(long adminId);
    }
}
