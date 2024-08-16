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
        public List<Admin> GetAllAdmins(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<Admin> GetAdminsOfDiscount(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Admin GetAdminById(long adminId);
        public void AddAdmin(Admin admin);
        public void EditAdmin(Admin admin);
        public void RemoveAdmin(Admin admin);
        public void RemoveAdmin(long adminId);
        public bool ExistAdmin(long adminId);
    }
}
