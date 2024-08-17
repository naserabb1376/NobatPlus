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
    public class CustomerDiscountRep : ICustomerDiscountRep
    {

        private NobatPlusContext _context;
        public CustomerDiscountRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddCustomerDiscountAsync(CustomerDiscount CustomerDiscount)
        {
            _context.CustomerDiscounts.Add(CustomerDiscount);
            await _context.SaveChangesAsync();
            _context.Entry(CustomerDiscount).State = EntityState.Detached;
        }

        public async Task EditCustomerDiscountAsync(CustomerDiscount CustomerDiscount)
        {
            _context.CustomerDiscounts.Update(CustomerDiscount);
            await _context.SaveChangesAsync();
            _context.Entry(CustomerDiscount).State = EntityState.Detached;
        }

        public async Task<bool> ExistCustomerDiscountAsync(long CustomerDiscountId)
        {
            return await _context.CustomerDiscounts
                .AsNoTracking()
                .AnyAsync(x => x.ID == CustomerDiscountId);
        }

        public async Task<List<CustomerDiscount>> GetAllCustomerDiscountsAsync(long DiscountId, long CustomerId, long StylistId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.CustomerDiscounts
                .AsNoTracking()
                .Include(x => x.Discount)
                .Include(x => x.Customer).ThenInclude(x => x.Person)
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Where(x =>
                    (x.DiscountId == DiscountId && x.CustomerId == CustomerId && x.StylistId == StylistId) &&
                    (
                        (!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountAmount.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<CustomerDiscount> GetCustomerDiscountByIdAsync(long CustomerDiscountId)
        {
            return await _context.CustomerDiscounts
                .AsNoTracking()
                .SingleOrDefaultAsync(x=> x.ID == CustomerDiscountId);
        }

        public async Task RemoveCustomerDiscountAsync(CustomerDiscount CustomerDiscount)
        {
            _context.CustomerDiscounts.Remove(CustomerDiscount);
            await _context.SaveChangesAsync();
            _context.Entry(CustomerDiscount).State = EntityState.Detached;
        }

        public async Task RemoveCustomerDiscountAsync(long CustomerDiscountId)
        {
            var CustomerDiscount = await GetCustomerDiscountByIdAsync(CustomerDiscountId);
            await RemoveCustomerDiscountAsync(CustomerDiscount);
        }
    }
}
