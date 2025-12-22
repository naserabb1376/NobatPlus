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

        public async Task<BitResultObject> ExistCustomerAsync(string fieldValue, string fieldName)
        {
            BitResultObject result = new BitResultObject();
            long customerid = 0;
            try
            {
                switch (fieldName.ToLower().Trim())
                {
                    case "personid":
                        {
                            var theCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.PersonID == long.Parse(fieldValue)) ?? new Customer();
                            customerid = theCustomer.ID;
                            break;
                        }
                    case "customerid":
                    default:
                        {
                            var theCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.ID == long.Parse(fieldValue)) ?? new Customer();
                            customerid = theCustomer.ID;
                            break;
                        }
                }
                result.ID = customerid;
                result.Status = customerid > 0;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<CustomerDTO>> GetAllCustomersAsync(
       long stylistId = 0,
       long cityId = 0,
       long discountId = 0,
       int pageIndex = 1,
       int pageSize = 20,
       string searchText = "",
       string sortQuery = "")
        {
            ListResultObject<CustomerDTO> results = new ListResultObject<CustomerDTO>();

            try
            {
                IQueryable<Customer> query;

                // اگر DiscountId مشخص شده بود، مشتریان مربوط به آن تخفیف را بگیر
                if (discountId > 0)
                {
                    query = _context.CustomerDiscounts
                        .AsNoTracking()
                        .Where(cd => cd.DiscountId == discountId)
                        .Select(cd => cd.Customer)
                        .Include(x => x.Person)
                            .ThenInclude(p => p.Address)
                                .ThenInclude(a => a.City)
                        .Include(x => x.Bookings)
                        .AsQueryable();
                }
                else
                {
                    // در غیر این صورت، همه مشتری‌ها
                    query = _context.Customers
                        .AsNoTracking()
                        .Include(x => x.Person)
                            .ThenInclude(p => p.Address)
                                .ThenInclude(a => a.City)
                        .Include(x => x.Bookings)
                        .AsQueryable();
                }

                if (stylistId > 0)
                {
                    query = query.Where(c =>
                        c.Bookings.Any(b =>
                            b.StylistID == stylistId && !b.IsCancelled
                        )
                    );
                }


                // فیلتر شهر
                if (cityId > 0)
                {
                    query = query.Where(x => x.Person.Address.CityID == cityId);
                }

                // فیلتر جستجو
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x =>
                        (x.Person.FirstName != null && x.Person.FirstName.Contains(searchText)) ||
                        (x.Person.LastName != null && x.Person.LastName.Contains(searchText)) ||
                        (x.Person.NaCode != null && x.Person.NaCode.Contains(searchText)) ||
                        (x.Person.PhoneNumber != null && x.Person.PhoneNumber.Contains(searchText)) ||
                        (x.Person.Email != null && x.Person.Email.Contains(searchText)) ||
                        (x.Person.Address.City.CityName != null && x.Person.Address.City.CityName.Contains(searchText)) ||
                        (x.Description != null && x.Description.Contains(searchText))
                    );
                }

                // ساخت خروجی با DTO
                var customerQuery = query.Select(x => new CustomerDTO
                {
                    ID = x.ID,
                    Person = x.Person,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate,
                    PersonID = x.PersonID,
                    Description = x.Description,
                    BookingCount = stylistId > 0
                        ? x.Bookings.Count(b => b.StylistID == stylistId)
                        : x.Bookings.Count(),

                    LastBookingDate = stylistId > 0
                        ? x.Bookings
                            .Where(b => b.StylistID == stylistId)
                            .OrderByDescending(b => b.BookingDate)
                            .Select(b => (DateTime?)b.BookingDate)
                            .FirstOrDefault()
                        : x.Bookings
                            .OrderByDescending(b => b.BookingDate)
                            .Select(b => (DateTime?)b.BookingDate)
                            .FirstOrDefault()
                });

                // شمارش کل و صفحه‌بندی
                results.TotalCount = await customerQuery.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await customerQuery
                    .OrderByDescending(x => x.LastBookingDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return results;
        }





        public async Task<RowResultObject<CustomerDTO>> GetCustomerByIdAsync(long CustomerId)
        {
            RowResultObject<CustomerDTO> result = new RowResultObject<CustomerDTO>();

            try
            {
                result.Result = await _context.Customers
                    .AsNoTracking()
                    .Where(x => x.ID == CustomerId)
                    .Select(x => new CustomerDTO
                    {
                        ID = x.ID,
                        PersonID = x.PersonID,
                        Person = x.Person,
                        BookingCount = _context.Bookings.Count(b => b.CustomerID == x.ID),
                        LastBookingDate = _context.Bookings
                            .Where(b => b.CustomerID == x.ID)
                            .OrderByDescending(b => b.BookingDate)
                            .Select(b => (DateTime?)b.BookingDate)
                            .FirstOrDefault()
                    })
                    .SingleOrDefaultAsync();

                if (result.Result == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "مشتری یافت نشد.";
                }
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
                var CustomerDto = await GetCustomerByIdAsync(CustomerId);
                var theCustomer = new Customer()
                {
                    CreateDate = CustomerDto.Result.CreateDate,
                    Description = CustomerDto.Result.Description,
                    PersonID = CustomerDto.Result.PersonID,
                    UpdateDate = CustomerDto.Result.UpdateDate,
                    ID = CustomerDto.Result.ID
                };
                result = await RemoveCustomerAsync(theCustomer);
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
