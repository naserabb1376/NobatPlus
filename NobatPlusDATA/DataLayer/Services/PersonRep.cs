using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
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

        public async Task<BitResultObject> AddPersonAsync(Person person)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Persons.AddAsync(person);
                await _context.SaveChangesAsync();
                result.ID = person.ID;
                _context.Entry(person).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> EditPersonAsync(Person person)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Persons.Update(person);
                await _context.SaveChangesAsync();
                result.ID = person.ID;
                _context.Entry(person).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> ExistPersonAsync(long personId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Persons
                .AsNoTracking()
                .AnyAsync(x => x.ID == personId);
                result.ID = personId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<ListResultObject<Person>> GetAllPersonsAsync(long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Person> results = new ListResultObject<Person>();
            try
            {
                IQueryable<Person> query = _context.Persons.Include(x => x.Address).ThenInclude(x => x.City).AsNoTracking(); ;

                if (cityId > 0)
                {
                    query = query.Where(x=> x.Address.CityID == cityId );
                }

               query = query.Where(x =>
                    (!string.IsNullOrEmpty(x.FirstName) && x.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.LastName) && x.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.NaCode) && x.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Email) && x.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Address.City.CityName.ToString()) && x.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Address.AddressLocationHorizentalPoint.ToString()) && x.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Address.AddressLocationVerticalPoint.ToString()) && x.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Address.AddressPostalCode.ToString()) && x.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Address.Description.ToString()) && x.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Address.AddressStreet.ToString()) && x.Address.AddressStreet.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.DateOfBirth.ToString()) && x.DateOfBirth.ToString().Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;

        }

        public async Task<RowResultObject<Person>> GetPersonByIdAsync(long personId)
        {
            RowResultObject<Person> result = new RowResultObject<Person>();
            try
            {
                result.Result = await _context.Persons.Include(x=> x.Address).ThenInclude(x => x.City)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == personId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemovePersonAsync(Person person)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
                result.ID = person.ID;
                _context.Entry(person).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemovePersonAsync(long personId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var person = await GetPersonByIdAsync(personId);
                result = await RemovePersonAsync(person.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }
    }
}
