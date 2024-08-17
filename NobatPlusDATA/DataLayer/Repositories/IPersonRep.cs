using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPersonRep
    {
        public Task<List<Person>> GetAllPersonsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Person> GetPersonByIdAsync(long personId);
        public Task AddPersonAsync(Person person);
        public Task EditPersonAsync(Person person);
        public Task RemovePersonAsync(Person person);
        public Task RemovePersonAsync(long personId);
        public Task<bool> ExistPersonAsync(long personId);
    }
}
