using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAdminRep
    {
        public Task<List<Admin>> GetAllAdminsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<Admin>> GetAdminsOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Admin> GetAdminByIdAsync(long adminId);
        public Task AddAdminAsync(Admin admin);
        public Task EditAdminAsync(Admin admin);
        public Task RemoveAdminAsync(Admin admin);
        public Task RemoveAdminAsync(long adminId);
        public Task<bool> ExistAdminAsync(long adminId);
    }
}
