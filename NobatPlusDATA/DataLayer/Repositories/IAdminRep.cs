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
        public Task<ListResultObject<Admin>> GetAllAdminsAsync(string role ="",long CityId =0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<ListResultObject<Admin>> GetAdminsOfDiscountAsync(long DiscountId,long cityId=0 ,string role = "", int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<Admin>> GetAdminByIdAsync(long adminId);
        public Task<BitResultObject> AddAdminAsync(Admin admin);
        public Task<BitResultObject> EditAdminAsync(Admin admin);
        public Task<BitResultObject> RemoveAdminAsync(Admin admin);
        public Task<BitResultObject> RemoveAdminAsync(long adminId);
        public Task<BitResultObject> ExistAdminAsync(long adminId);
    }
}
