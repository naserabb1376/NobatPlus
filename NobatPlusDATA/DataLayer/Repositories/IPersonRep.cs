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
        public List<Person> GetAllPersons(int pageIndex = 1,int pagesize = 20, string searchText ="");
        public Person GetPersonById(int personId);
        public void AddPerson(Person person);
        public void EditPerson(Person person);
        public void RemovePerson(Person person);
        public void RemovePerson(int personId);
        public bool ExistPerson(int personId);
    }
}
