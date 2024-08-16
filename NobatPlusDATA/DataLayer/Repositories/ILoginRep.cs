using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ILoginRep
    {
        public List<Login> GetAllLogins(long personId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Login GetLoginById(long LoginId);
        public void AddLogin(Login Login);
        public void EditLogin(Login Login);
        public void RemoveLogin(Login Login);
        public void RemoveLogin(long LoginId);
        public bool ExistLogin(long LoginId);
    }
}
