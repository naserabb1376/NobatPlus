using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IRegisterRep
    {
        public Task<List<Register>> GetAllRegistersAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Register> GetRegisterByIdAsync(long RegisterId);
        public Task<Register> GetRegisterByPersonIdAsync(long PersonId);
        public Task AddRegisterAsync(Register Register);
        public Task EditRegisterAsync(Register Register);
        public Task RemoveRegisterAsync(Register Register);
        public Task RemoveRegisterAsync(long RegisterId);
        public Task<bool> ExistRegisterAsync(long RegisterId);
    }
}
