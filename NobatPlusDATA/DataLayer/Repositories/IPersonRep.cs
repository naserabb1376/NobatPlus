using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPersonRep
    {
        public Task<ListResultObject<Person>> GetAllPersonsAsync(long cityId = 0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<Person>> GetPersonByIdAsync(long personId);
        public Task<BitResultObject> AddPersonAsync(Person person);
        public Task<BitResultObject> EditPersonAsync(Person person);
        public Task<BitResultObject> RemovePersonAsync(Person person);
        public Task<BitResultObject> RemovePersonAsync(long personId);
        public Task<BitResultObject> ExistPersonAsync(long personId);
    }
}
