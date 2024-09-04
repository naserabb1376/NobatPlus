using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ILoginRep
    {
        public Task<ListResultObject<Login>> GetAllLoginsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<Login>> GetLoginByIdAsync(long LoginId);
        public Task<BitResultObject> AddLoginAsync(Login Login);
        public Task<RowResultObject<Login>> AuthenticateAsync(string userName,string password,int authenticateType = 1);
        public Task<BitResultObject> EditLoginAsync(Login Login);
        public Task<BitResultObject> RemoveLoginAsync(Login Login);
        public Task<BitResultObject> RemoveLoginAsync(long LoginId);
        public Task<BitResultObject> ExistLoginAsync(string uniqueProperty,int searchMode = 1);
    }
}
