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
        public List<Register> GetAllRegisters(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Register GetRegisterById(long RegisterId);
        public Register GetRegisterByPersonId(long PersonId);
        public void AddRegister(Register Register);
        public void EditRegister(Register Register);
        public void RemoveRegister(Register Register);
        public void RemoveRegister(long RegisterId);
        public bool ExistRegister(long RegisterId);
    }
}
