using Domain;
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
    public class CustomerRep : ICustomerRep
    {

        private NobatPlusContext _context;
        public CustomerRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddCustomerAsync(Customer Customer)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Customers.AddAsync(Customer);
                await _context.SaveChangesAsync();
                result.ID = Customer.ID;
                _context.Entry(Customer).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditCustomerAsync(Customer Customer)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Customers.Update(Customer);
                await _context.SaveChangesAsync();
                result.ID = Customer.ID;
                _context.Entry(Customer).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistCustomerAsync(long CustomerId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Customers
                .AsNoTracking()
                .AnyAsync(x => x.ID == CustomerId);
                result.ID = CustomerId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<Customer>> GetAllCustomersAsync(long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Customer> results = new ListResultObject<Customer>();
            try
            {
                var query = _context.Customers
                .AsNoTracking()
                .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||

                    (x.Person.DateOfBirth != null && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
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

        public async Task<ListResultObject<Customer>> GetCustomersOfDiscountAsync(long DiscountId,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Customer> results = new ListResultObject<Customer>();
            try
            {
                IQueryable<Customer> query;
                 query = _context.CustomerDiscounts
                .AsNoTracking()
                .Where(bs => bs.DiscountId == DiscountId)
                .Select(bs => bs.Customer)
                .Include(x => x.Person).ThenInclude(x=> x.Address).ThenInclude(x=> x.City)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth != null && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
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

        public async Task<RowResultObject<Customer>> GetCustomerByIdAsync(long CustomerId)
        {
            RowResultObject<Customer> result = new RowResultObject<Customer>();
            try
            {
                result.Result = await _context.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == CustomerId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
      
        }

        public async Task<BitResultObject> RemoveCustomerAsync(Customer Customer)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Customers.Remove(Customer);
                await _context.SaveChangesAsync();
                result.ID = Customer.ID;
                _context.Entry(Customer).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveCustomerAsync(long CustomerId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Customer = await GetCustomerByIdAsync(CustomerId);
                result = await RemoveCustomerAsync(Customer.Result);
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
