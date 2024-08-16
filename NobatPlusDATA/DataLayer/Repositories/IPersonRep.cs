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
        public List<Person> GetAllPersons(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Person GetPersonById(long personId);
        public void AddPerson(Person person);
        public void EditPerson(Person person);
        public void RemovePerson(Person person);
        public void RemovePerson(long personId);
        public bool ExistPerson(long personId);
    }
}
