using Domain;
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
    public class CustomerRep : ICustomerRep
    {

        private NobatPlusContext _context;
        public CustomerRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddCustomerAsync(Customer Customer)
        {
            _context.Customers.Add(Customer);
            await _context.SaveChangesAsync();
            _context.Entry(Customer).State = EntityState.Detached;
        }

        public async Task EditCustomerAsync(Customer Customer)
        {
            _context.Customers.Update(Customer);
            await _context.SaveChangesAsync();
            _context.Entry(Customer).State = EntityState.Detached;
        }

        public async Task<bool> ExistCustomerAsync(long CustomerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(x => x.ID == CustomerId);
        }

        public async Task<List<Customer>> GetAllCustomersAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Customers
                .AsNoTracking()
                .Include(x => x.Person)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<Customer>> GetCustomersOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.CustomerDiscounts
                .AsNoTracking()
                .Where(bs => bs.DiscountId == DiscountId)
                .Select(bs => bs.Customer)
                .Include(x => x.Person)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(long CustomerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(x=> x.ID == CustomerId);
        }

        public async Task RemoveCustomerAsync(Customer Customer)
        {
            _context.Customers.Remove(Customer);
            await _context.SaveChangesAsync();
            _context.Entry(Customer).State = EntityState.Detached;
        }

        public async Task RemoveCustomerAsync(long CustomerId)
        {
            var Customer = await GetCustomerByIdAsync(CustomerId);
            await RemoveCustomerAsync(Customer);
        }
    }
}
