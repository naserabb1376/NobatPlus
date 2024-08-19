using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IRegisterRep
    {
        public Task<ListResultObject<Register>> GetAllRegistersAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<Register>> GetRegisterByIdAsync(long RegisterId);
        public Task<RowResultObject<Register>> GetRegisterByPersonIdAsync(long PersonId);
        public Task<BitResultObject> AddRegisterAsync(Register Register);
        public Task<BitResultObject> EditRegisterAsync(Register Register);
        public Task<BitResultObject> RemoveRegisterAsync(Register Register);
        public Task<BitResultObject> RemoveRegisterAsync(long RegisterId);
        public Task<BitResultObject> ExistRegisterAsync(long RegisterId);
    }
}
