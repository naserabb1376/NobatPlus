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

        public async Task AddPersonAsync(Person person)
        {
            _context.Persons.Add(person);
            await _context.SaveChangesAsync();
            _context.Entry(person).State = EntityState.Detached;
        }

        public async Task EditPersonAsync(Person person)
        {
            _context.Persons.Update(person);
            await _context.SaveChangesAsync();
            _context.Entry(person).State = EntityState.Detached;
        }

        public async Task<bool> ExistPersonAsync(long personId)
        {
            return await _context.Persons
                .AsNoTracking()
                .AnyAsync(x => x.ID == personId);
        }

        public async Task<List<Person>> GetAllPersonsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Persons
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.FirstName) && x.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.LastName) && x.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.NaCode) && x.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Email) && x.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (x.DateOfBirth.HasValue && x.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<Person> GetPersonByIdAsync(long personId)
        {
            return await _context.Persons
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == personId);
        }

        public async Task RemovePersonAsync(Person person)
        {
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
            _context.Entry(person).State = EntityState.Detached;
        }

        public async Task RemovePersonAsync(long personId)
        {
            var person = await GetPersonByIdAsync(personId);
            await RemovePersonAsync(person);
        }
    }
}
