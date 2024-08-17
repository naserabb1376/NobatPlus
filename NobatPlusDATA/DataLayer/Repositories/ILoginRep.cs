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
        public Task<List<Login>> GetAllLoginsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Login> GetLoginByIdAsync(long LoginId);
        public Task AddLoginAsync(Login Login);
        public Task EditLoginAsync(Login Login);
        public Task RemoveLoginAsync(Login Login);
        public Task RemoveLoginAsync(long LoginId);
        public Task<bool> ExistLoginAsync(long LoginId);
    }
}
