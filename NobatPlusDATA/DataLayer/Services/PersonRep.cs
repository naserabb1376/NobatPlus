using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class PersonRep : IPersonRep
    {

        private NobatPlusContext _context;
        public PersonRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddPerson(Person person)
        {
            _context.Persons.Add(person);
            _context.SaveChanges();
            _context.Entry(person).State = EntityState.Detached;
        }

        public void EditPerson(Person person)
        {
            _context.Persons.Update(person);
            _context.SaveChanges();
            _context.Entry(person).State = EntityState.Detached;
        }

        public bool ExistPerson(long personId)
        {
            return _context.Persons.Any(x => x.ID == personId);
        }

        public List<Person> GetAllPersons(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.Persons.Where(x =>
            (!string.IsNullOrEmpty(x.FirstName.ToString()) && x.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.LastName.ToString()) && x.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.NaCode.ToString()) && x.NaCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.PhoneNumber.ToString()) && x.PhoneNumber.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Email.ToString()) && x.Email.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.DateOfBirth.ToString()) && x.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex,pageSize).ToList();
        }

        public Person GetPersonById(long personId)
        {
            return _context.Persons.Find(personId);
        }

        public void RemovePerson(Person person)
        {
            _context.Persons.Remove(person);
            _context.SaveChanges();
            _context.Entry(person).State = EntityState.Detached;
        }

        public void RemovePerson(long personId)
        {
            var person = GetPersonById(personId);
            RemovePerson(person);
        }
    }
}
